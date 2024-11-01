using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.AspNetCore.SignalR;

namespace SignalRWebpack.Hubs;

class GameHub(
    GameState<GameHub> gameState,IDbConnection db,
    IPlayerRepository playerRepository
) : Hub{

    public async override Task<Task> OnConnectedAsync()
    {
        
        var newPlayer = new SocketPlayer{SocketId = Context.ConnectionId};
        gameState.Players.Add(newPlayer);
        await Clients.Caller.SendAsync("UpdateCurrentPlayer", newPlayer);

        var activePlayers = await playerRepository.Search(new PlayerSearchParams { Active = true});

        var getGame = "SELECT * FROM GAME";
        var game = await db.QueryFirstOrDefaultAsync<Game>(getGame);
        await Clients.Caller.SendAsync("OnConnectGameState", game);
        await Clients.All.SendAsync("UpdatePlayers", activePlayers);
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

        gameState.RemovePlayer(Context.ConnectionId);
        await Clients.All.SendAsync("UpdatePlayers", gameState.Players);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task ReconnectPlayer(string playerId)
    {
        var currentPlayer = gameState.GetPlayer(Context.ConnectionId);
        currentPlayer.PlayerId = playerId;
        await playerRepository.Update(currentPlayer.PlayerId, new PlayerUpdateParams {Active = true});
        var activePlayers = await playerRepository.Search(new PlayerSearchParams { Active = true});

        await Clients.All.SendAsync("UpdatePlayers",activePlayers);
        await Clients.Caller.SendAsync("UpdateCurrentPlayer", currentPlayer);
    }

    public async Task AddNewPlayer(string name, int iconId)
    {
        SocketPlayer currentPlayer = gameState.GetPlayer(Context.ConnectionId);

        var uuid = Guid.NewGuid().ToString();
        
        var addNewPlayer = @"
            INSERT INTO Player (Id,PlayerName,IconId)
            VALUES (@Id,@PlayerName, @IconId)
        ";

        var parameters = new 
        {
            Id = uuid,
            PlayerName = name,
            IconId = iconId,
        };

        await db.ExecuteAsync(addNewPlayer,parameters);

        var newPlayer = await playerRepository.GetByIdAsync(uuid);

        currentPlayer.PlayerId = newPlayer!.Id;

        var activePlayers = await playerRepository.Search(new PlayerSearchParams {Active = true});
        
        await Clients.Caller.SendAsync("UpdateCurrentPlayer",currentPlayer);
        await Clients.All.SendAsync("UpdatePlayers",activePlayers);
    }

    public async Task UpdatePlayer(string playerId, PlayerUpdateParams playerUpdateParams)
    {
        await playerRepository.Update(playerId, playerUpdateParams);
        var activePlayers = await playerRepository.Search(new PlayerSearchParams { Active = true});
        await Clients.All.SendAsync("UpdatePlayers",activePlayers);
    }
}