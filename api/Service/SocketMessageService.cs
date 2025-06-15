using api.DTO.Entity;
using api.Entity;
using api.Enumerable;
using api.hub;
using api.Interface;
using api.Repository;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
namespace api.Service;

public interface ISocketMessageService
{
    public Task SendToSelf(WebSocketEvents eventEnum, object? data);
    public Task SendToGroup(WebSocketEvents eventEnum, object? data);
    public Task SendToAll(WebSocketEvents eventEnum, object? data);
    public Task SendLatestGameLogs(Guid gameId);
    public Task CreateAndSendLatestGameLogs(Guid gameId, string message);
    public Task SendGamePlayers(Guid gameId, bool includeLatestLogs = true);
    public Task SendGameBoardSpaces(Guid gameId);
}

public class SocketMessageService(
    GameState<MonopolyHub> gameState,
    ISocketContextAccessor socketContext,
    IGameLogRepository gameLogRepository,
    IPlayerRepository playerRepository,
    IBoardSpaceRepository boardSpaceRepository
) : ISocketMessageService
{
    public async Task SendToSelf(WebSocketEvents eventEnum, object? data)
    {
        if (socketContext.Current == null)
        {
            throw new Exception("Socket context is null in SendToSelf");
        }
        await socketContext.Current.Clients.Caller.SendAsync(((int)eventEnum).ToString(), data);
    }
    public async Task SendToGroup(WebSocketEvents eventEnum, object? data)
    {
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
        if (socketContext.Current == null)
        {
            throw new Exception("Socket context is null in SendToAll");
        }
        await socketContext.Current.Clients.All.SendAsync(((int)eventEnum).ToString(), data);
    }

    public async Task SendLatestGameLogs(Guid gameId)
    {
        IEnumerable<GameLog> latestLogs = await gameLogRepository.GetLatestFive(gameId);
        await SendToGroup(WebSocketEvents.GameLogUpdate, latestLogs);
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
        IEnumerable<BoardSpace> boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameId);
        await SendToGroup(WebSocketEvents.BoardSpaceUpdate, boardSpaces);
    }
}
