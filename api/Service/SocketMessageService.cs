using api.Enumerable;
using api.hub;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
namespace api.Service;

public interface ISocketMessageService
{
    public Task SendToSelf(WebSocketEvents eventEnum, object? data);
    public Task SendToGroup(WebSocketEvents eventEnum, object? data);
    public Task SendToAll(WebSocketEvents eventEnum, object? data);
}

public class SocketMessageService(
    GameState<MonopolyHub> gameState,
    ISocketContextAccessor socketContext
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
}
