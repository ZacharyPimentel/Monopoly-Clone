using api.hub;
using Microsoft.AspNetCore.SignalR;

namespace api.Socket;

public class SocketContext<THub> where THub : MonopolyHub
{
    public HubCallerContext Context { get; init; } = default!;
    public IHubContext<MonopolyHub> HubContext { get; init; } = default!;
    public IHubCallerClients Clients { get; init; } = default!;
}