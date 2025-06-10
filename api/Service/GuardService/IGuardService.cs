using api.Entity;
namespace api.Service.GuardService;

public interface IGuardService
{
    public Player GetPlayer();
    public Game GetGame();
    public Task HandleGuardError( Func<Task> action);
    public Task<IGuardClause> Init(Guid? playerId = null, Guid? gameId = null);
    public IGuardService SocketConnectionHasPlayerId();
    public IGuardService SocketConnectionDoesNotHavePlayerId();
    public IGuardService SocketConnectionHasGameId();
    public IGuardService SocketConnectionDoesNotHaveGameId();
    
}