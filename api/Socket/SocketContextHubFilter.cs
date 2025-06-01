using api.hub;
using Microsoft.AspNetCore.SignalR;

namespace api.Socket;
public class SocketContextHubFilter<THub>(ISocketContextAccessor socketContext) : IHubFilter where THub : Microsoft.AspNetCore.SignalR.Hub
{
    public async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        // Set the SocketContext before the method is executed
        socketContext.Current = new SocketContext<MonopolyHub>
        {
            Context = invocationContext.Context,
            Clients = invocationContext.Hub.Clients
        };

        return await next(invocationContext); // Continue to the hub method
    }

    public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
    {
        await next(context); // Optional: could also set context here if desired
    }

    public async Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Task> next)
    {
        await next(context); // Optional cleanup
    }
}
