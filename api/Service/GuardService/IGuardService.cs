using api.Entity;
namespace api.Service.GuardService;

public interface IGuardService
{
    public Player GetPlayer();
    public Guid GetPlayerId();
    public IEnumerable<Player> GetPlayers();
    public Player GetCurrentPlayerFromList();
    public Player GetPlayerFromList(Guid playerId);
    public Game GetGame();
    public Guid GetGameId();
    public Task HandleGuardError(Func<Task> action);
    public Task<IGuardClause> Init(Guid? playerId = null, Guid? gameId = null);
    public Task<IGuardClause> InitMultiple(IEnumerable<Guid> playerIds, Guid? gameId = null);
    public IGuardService SocketConnectionHasPlayerId();
    public IGuardService SocketConnectionDoesNotHavePlayerId();
    public IGuardService SocketConnectionHasGameId();
    public IGuardService SocketConnectionDoesNotHaveGameId();
}