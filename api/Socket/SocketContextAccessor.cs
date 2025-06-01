using api.hub;
using api.Socket;
namespace api.Socket;

public interface ISocketContextAccessor
{
    SocketContext<MonopolyHub>? Current { get; set; }
    SocketContext<MonopolyHub> RequireContext();
}

public class SocketContextAccessor : ISocketContextAccessor
{
    public SocketContext<MonopolyHub>? Current { get; set; }

    public SocketContext<MonopolyHub> RequireContext()
    {
        return Current ?? throw new InvalidOperationException("SocketContext has not been set for this request.");
    }
}
