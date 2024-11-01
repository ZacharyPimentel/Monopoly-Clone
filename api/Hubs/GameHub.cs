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

        var allPlayers = await playerRepository.GetAllAsync();

        var getGame = "SELECT * FROM GAME";
        var game = await db.QueryFirstOrDefaultAsync<Game>(getGame);
        await Clients.Caller.SendAsync("OnConnectGameState", game);
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
        await Clients.All.SendAsync("UpdatePlayers",allPlayers);
    }

    public async Task SetLastDiceRoll(int[] rolls)
    {
        gameState.SetLastDiceRoll(rolls);
        await Clients.All.SendAsync("UpdateLastDiceRoll",rolls);
    }
}