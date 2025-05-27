namespace api.Interface;

public class GameLogCreateParams
{
    public required Guid GameId { get; set; }
    public required string Message { get; set; }
}
public interface IGameLogRepository : IBaseRepository<GameLog,int>
{
    Task<List<GameLog>> GetLatestFive(Guid gameId);
    Task<List<GameLog>> GetAll(Guid gameId);
}