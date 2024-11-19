using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.AspNetCore.SignalR;

namespace SignalRWebpack.Hubs;

public class MonopolyHub(
    GameState<MonopolyHub> gameState,
    IPlayerRepository playerRepository,
    IGameRepository gameRepository,
    IDbConnection db
) : Hub{

    //=======================================================
    // Default socket methods for connect / disconnect
    //=======================================================
    public override Task OnConnectedAsync()
    {
        var newPlayer = new SocketPlayer{SocketId = Context.ConnectionId};
        gameState.Players.Add(newPlayer);
        return base.OnConnectedAsync();
    }
    public override async Task<Task> OnDisconnectedAsync(Exception? exception)
    {
        var currentPlayer = gameState.GetPlayer(Context.ConnectionId);
        //if the socket player has a game player id, update to inactive
        if(currentPlayer.PlayerId != null && currentPlayer.GameId != null)
        {
            await playerRepository.Update(currentPlayer.PlayerId, new PlayerUpdateParams {Active = false});
            var groupPlayers = await playerRepository.Search( new PlayerWhereParams {GameId = currentPlayer.GameId});
            await SendToGroup("player:updateGroup", groupPlayers);
        }
        gameState.RemovePlayer(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
    //=======================================================
    // Methods for message delivery (self, group, or all)
    //=======================================================
    private async Task SendToSelf(string eventName, object data)
    {
        await Clients.Caller.SendAsync(eventName,data);
    }
    private async Task SendToGroup(string eventName, object data)
    {
        SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
        if(currentSocketPlayer.GameId != null)
        {
            await Clients.Group(currentSocketPlayer.GameId).SendAsync(eventName, data);
        }else{
            throw new Exception("Tried to send data to a group where the GameId was not found.");
        }

        await Clients.Caller.SendAsync(eventName,data);
    }
    private async Task SendToAll(string eventName, object data)
    {
        await Clients.All.SendAsync(eventName, data);
    }

    //=======================================================
    // Player 
    //=======================================================
    public async Task PlayerReconnect(string playerId)
    {
        var socketPlayer = gameState.GetPlayer(Context.ConnectionId);
        await playerRepository.Update(playerId, new PlayerUpdateParams {Active = true});
        socketPlayer.PlayerId = playerId;
        var allPlayers = await playerRepository.GetAllAsync();
        await SendToSelf("player:update", socketPlayer);
        await SendToGroup("player:updateGroup",allPlayers);

        //trigger updated player counts in lobby
        var games = await gameRepository.Search(new GameWhereParams{});
        await SendToAll("game:updateAll",games);
    }
    public async Task PlayerCreate(PlayerCreateParams playerCreateParams)
    {
        SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
        Player newPlayer = await playerRepository.Create(playerCreateParams);
        currentSocketPlayer.PlayerId = newPlayer.Id;
        var groupPlayers = await playerRepository.Search(new PlayerWhereParams {
            GameId = currentSocketPlayer.GameId
        });
        await SendToSelf("player:update",currentSocketPlayer);
        await SendToGroup("player:updateGroup",groupPlayers);

        //trigger updated player counts in lobby
        var games = await gameRepository.Search(new GameWhereParams{});
        await SendToAll("game:updateAll",games);
    }
    public async Task PlayerUpdate(string playerId,PlayerUpdateParams playerUpdateParams)
    {
        await playerRepository.Update(playerId, playerUpdateParams);
        SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
        if(currentSocketPlayer.GameId == null)
        {
            throw new Exception("Player does not have a GameId when it should be there.");
        }

        Game currentGame = await gameRepository.GetByIdAsync(currentSocketPlayer.GameId);
        var activeGroupPlayers = await playerRepository.Search( new PlayerWhereParams{
            Active = true,
            GameId = currentGame.Id
        });

        //if at least two players are all ready and the game is in lobby, start the game
        if (currentGame.InLobby && activeGroupPlayers.All(x => x.IsReadyToPlay == true) && activeGroupPlayers.AsList().Count >= 2)
        {
            await gameRepository.Update(currentGame.Id, new GameUpdateParams{
                InLobby = false,
                GameStarted = true
            });
            await playerRepository.UpdateMany(
                new PlayerWhereParams {Active = true},
                new PlayerUpdateParams {
                    InCurrentGame = true,
                    IsReadyToPlay = false,
                    Money = currentGame.StartingMoney
                }
            );

            //randomize and set the turn order
            Random random = new();
            var shuffledActivePlayers = activeGroupPlayers.OrderBy(x => random.Next()).ToArray();
            foreach(var (player,index) in shuffledActivePlayers.Select( (value,i) => (value,i)))
            {
                var addTurnOrder = @"
                    INSERT INTO TurnOrder (Id,PlayerId,GameId,PlayOrder)
                    VALUES (@Id,@PlayerId, @GameId,@PlayOrder)
                ";

                var parameters = new TurnOrder
                {
                    Id = Guid.NewGuid().ToString(),
                    PlayerId = player.Id,
                    GameId = currentGame.Id,
                    PlayOrder = index + 1
                };

                await db.ExecuteAsync(addTurnOrder,parameters);
            } 
            
            var updatedGame = await gameRepository.GetByIdAsync(currentGame.Id);
            await SendToGroup("game:update",updatedGame);
        }
        var updatedGroupPlayers = await playerRepository.Search(new PlayerWhereParams {
            GameId = currentSocketPlayer.GameId
        });
        
        await SendToGroup("player:updateGroup",updatedGroupPlayers);
    }
    
    //=======================================================
    // Game
    //=======================================================
    public async Task GameGetAll()
    {
        var games = await gameRepository.Search(new GameWhereParams{});
        await SendToSelf("game:updateAll", games);
    }
    public async Task GameGetById(string gameId)
    {
        Game game = await gameRepository.GetByIdAsync(gameId);
        await SendToSelf("game:update",game);
    }
    public async Task GameCreate(GameCreateParams gameCreateParams)
    {
        var newGame = await gameRepository.Create(gameCreateParams);
        await SendToSelf("game:create",newGame.Id);
        var games = await gameRepository.Search(new GameWhereParams{});
        await SendToAll("game:updateAll",games);
    }
    public async Task GameJoin(string gameId)
    {
        SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
        currentSocketPlayer.GameId = gameId;
        await Groups.AddToGroupAsync(Context.ConnectionId,gameId);
        Game game = await gameRepository.GetByIdAsync(gameId);
        var groupPlayers = await playerRepository.Search(new PlayerWhereParams {GameId = gameId});
        await SendToSelf("game:update",game);
        await SendToSelf("player:update",currentSocketPlayer);
        await SendToSelf("player:updateGroup",groupPlayers);
    }
    public async Task GameLeave(string gameId)
    {
        SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
        if(currentSocketPlayer.PlayerId != null && currentSocketPlayer.GameId != null)
        {
            await playerRepository.Update(currentSocketPlayer.PlayerId, new PlayerUpdateParams {Active = false});
            var groupPlayers = await playerRepository.Search( new PlayerWhereParams {GameId = currentSocketPlayer.GameId});
            await Groups.RemoveFromGroupAsync(Context.ConnectionId,gameId);
            await SendToGroup("player:updateGroup", groupPlayers);
            currentSocketPlayer.GameId = null;
            currentSocketPlayer.PlayerId = null;
            var games = await gameRepository.Search(new GameWhereParams{});
            await SendToAll("game:updateAll",games);
        }
    }
}