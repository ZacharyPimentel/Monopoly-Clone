using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.AspNetCore.SignalR;

namespace SignalRWebpack.Hubs;

class GameHub(
    GameState<GameHub> gameState,IDbConnection db,
    IPlayerRepository playerRepository,
    IGameRepository gameRepository
) : Hub{

    public async override Task<Task> OnConnectedAsync()
    {
        
        var newPlayer = new SocketPlayer{SocketId = Context.ConnectionId};
        gameState.Players.Add(newPlayer);
        await Clients.Caller.SendAsync("UpdateCurrentPlayer", newPlayer);

        var allPlayers = await playerRepository.GetAllAsync();

        var getGame = "SELECT * FROM GAME";
        var game = await db.QueryFirstOrDefaultAsync<Game>(getGame);
        await Clients.Caller.SendAsync("UpdateGameState", game);
        await Clients.All.SendAsync("UpdatePlayers", allPlayers);
        await Clients.All.SendAsync("UpdateLastDiceRoll",gameState.LastDiceRoll);
        return base.OnConnectedAsync();
    }

    public override async Task<Task> OnDisconnectedAsync(Exception? exception)
    {
        
        var currentPlayer = gameState.GetPlayer(Context.ConnectionId);

        //if the socket player has a game player id, update to inactive
        if(currentPlayer.PlayerId != null)
        {
            await playerRepository.Update(currentPlayer.PlayerId, new PlayerUpdateParams {Active = false});
        }

        var allPlayers = await playerRepository.GetAllAsync();
        gameState.RemovePlayer(Context.ConnectionId);
        await Clients.All.SendAsync("UpdatePlayers", allPlayers);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task ReconnectPlayer(string playerId)
    {
        var currentPlayer = gameState.GetPlayer(Context.ConnectionId);
        currentPlayer.PlayerId = playerId;
        await playerRepository.Update(playerId, new PlayerUpdateParams {Active = true});
        var allPlayers = await playerRepository.GetAllAsync();

        await Clients.All.SendAsync("UpdatePlayers",allPlayers);
        await Clients.Caller.SendAsync("UpdateCurrentPlayer", currentPlayer);
    }

    public async Task AddNewPlayer(string name, int iconId)
    {
        SocketPlayer currentPlayer = gameState.GetPlayer(Context.ConnectionId);

        var newPlayer = await playerRepository.Create(new PlayerCreateParams
        {
            PlayerName = name,
            IconId = iconId
        });

        currentPlayer.PlayerId = newPlayer.Id;

        var allPlayers = await playerRepository.GetAllAsync();

        await Clients.Caller.SendAsync("UpdateCurrentPlayer",currentPlayer);
        await Clients.All.SendAsync("UpdatePlayers",allPlayers);
    }

    public async Task UpdatePlayer(string playerId, PlayerUpdateParams playerUpdateParams)
    {
        await playerRepository.Update(playerId, playerUpdateParams);
        var allPlayers = await playerRepository.GetAllAsync();
        var activePlayers = allPlayers.Where(x => x.Active).ToList();
        var currentGame = await gameRepository.GetByIdAsync(1);
        
        await Clients.All.SendAsync("UpdatePlayers",allPlayers);
        
        //if at least two players are all ready and the game is in lobby, start the game
        if (currentGame.InLobby && activePlayers.All(x => x.IsReadyToPlay == true) && activePlayers.Count >= 2)
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

            var updatedGame = await gameRepository.GetByIdAsync(1);
            var updatedPlayers = await playerRepository.GetAllAsync();

            await Clients.All.SendAsync("UpdateGameState",updatedGame);
            await Clients.All.SendAsync("UpdatePlayers",updatedPlayers);
        }
    }
    public async Task UpdateRules(int gameId, GameUpdateParams gameUpdateParams)
    {
        await gameRepository.Update(gameId,gameUpdateParams);
        var updatedGame = await gameRepository.GetByIdAsync(gameId);
        await Clients.All.SendAsync("UpdateGameState",updatedGame);
    }

    public async Task SetLastDiceRoll(int[] rolls)
    {
        gameState.SetLastDiceRoll(rolls);
        await Clients.All.SendAsync("UpdateLastDiceRoll",rolls);
    }
}