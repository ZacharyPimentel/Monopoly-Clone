using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.hub;
using api.Hubs;
using api.Interface;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
namespace api.Service;

public class GameStateIncludeParams {
    public bool Game { get; set; } = false;
    public bool Players { get; set; } = false;
    public bool GameLogs { get; set; } = false;
    public bool BoardSpaces { get; set; } = false;
    public bool Trades { get; set; } = false;
}

public interface ISocketMessageService
{
    bool SuppressMessages { get; set; }
    public Task SendToSelf(WebSocketEvents eventEnum, object? data);
    public Task SendToGroup(WebSocketEvents eventEnum, object? data);
    public Task SendToAll(WebSocketEvents eventEnum, object? data);
    public Task SendLatestGameLogs(Guid gameId);
    public Task CreateAndSendLatestGameLogs(Guid gameId, string message);
    public Task SendGamePlayers(Guid gameId, bool includeLatestLogs = true);
    public Task SendGameBoardSpaces(Guid gameId);
    public Task SendGameStateUpdate (Guid gameId, GameStateIncludeParams includeParams);
}

public class SocketMessageService(
    GameState<MonopolyHub> gameState,
    ISocketContextAccessor socketContext,
    IGameLogRepository gameLogRepository,
    IPlayerRepository playerRepository,
    IBoardSpaceRepository boardSpaceRepository,
    IGameRepository gameRepository,
    ITradeRepository tradeRepository
) : ISocketMessageService
{
    public bool SuppressMessages { get; set; } = false;
    public async Task SendToSelf(WebSocketEvents eventEnum, object? data)
    {
        if (SuppressMessages) return;

        if (socketContext.Current == null)
        {
            throw new Exception("Socket context is null in SendToSelf");
        }
        await socketContext.Current.Clients.Caller.SendAsync(((int)eventEnum).ToString(), data);
    }
    public async Task SendToGroup(WebSocketEvents eventEnum, object? data)
    {
        if (SuppressMessages) return;

        if (socketContext.Current == null)
        {
            throw new Exception("Socket context is null in SendToGroup");
        }

        SocketPlayer currentSocketPlayer = gameState.GetPlayer(socketContext.Current.Context.ConnectionId);
        if (currentSocketPlayer.GameId is Guid gameId)
        {
            await socketContext.Current.Clients.Group(gameId.ToString()).SendAsync(((int)eventEnum).ToString(), data);
        }
        else
        {
            throw new Exception("Tried to send data to a group where the GameId was not found.");
        }
    }
    public async Task SendToAll(WebSocketEvents eventEnum, object? data)
    {
        if (SuppressMessages) return;

        if (socketContext.Current == null)
        {
            throw new Exception("Socket context is null in SendToAll");
        }
        await socketContext.Current.Clients.All.SendAsync(((int)eventEnum).ToString(), data);
    }

    public async Task SendLatestGameLogs(Guid gameId)
    {
        if (SuppressMessages) return;

        IEnumerable<GameLog> latestLogs = await gameLogRepository.GetLatestFive(gameId);
        var response = new GameStateResponse
        {
            GameLogs = latestLogs
        };

        await SendToGroup(WebSocketEvents.GameStateUpdate, response);
    }
    public async Task CreateAndSendLatestGameLogs(Guid gameId, string message)
    {
        await gameLogRepository.CreateAsync(new GameLogCreateParams
        {
            GameId = gameId,
            Message = message
        });
        await SendLatestGameLogs(gameId);
    }

    public async Task SendGamePlayers(Guid gameId, bool includeLatestLogs = true)
    {
        if (SuppressMessages) return;

        IEnumerable<Player> gamePlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams
        {
            GameId = gameId
        });
        if (includeLatestLogs)
        {
            await SendLatestGameLogs(gameId);
        }
        await SendToGroup(WebSocketEvents.PlayerUpdateGroup, gamePlayers);
    }
    public async Task SendGameBoardSpaces(Guid gameId)
    {
        if (SuppressMessages) return;

        IEnumerable<BoardSpace> boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameId);
        await SendToGroup(WebSocketEvents.BoardSpaceUpdate, boardSpaces);
    }

    public async Task SendGameStateUpdate(Guid gameId, GameStateIncludeParams includeParams)
    {
        if (SuppressMessages) return;

        GameStateResponse response = new();

        if (includeParams.Game)
        {
            response.Game = await gameRepository.GetByIdWithDetailsAsync(gameId);
        }
        if (includeParams.Players)
        {
            response.Players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = gameId });
        }
        if (includeParams.GameLogs)
        {
            response.GameLogs = await gameLogRepository.GetLatestFive(gameId);
        }
        if (includeParams.BoardSpaces)
        {
            response.BoardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameId);
        }
        if (includeParams.Trades)
        {
            response.Trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
        }

        await SendToGroup(WebSocketEvents.GameStateUpdate, response);
    }
}
