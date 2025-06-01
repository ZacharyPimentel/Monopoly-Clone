using api.hub;
using api.Socket;
namespace api.Socket;
public interface ISocketContextAccessor
{
    SocketContext<MonopolyHub>? Current { get; set; }
}

public class SocketContextAccessor : ISocketContextAccessor
{
    public SocketContext<MonopolyHub>? Current { get; set; }
}
