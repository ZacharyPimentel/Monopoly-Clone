public interface IGameLogRepository
{
    Task CreateLog (string gameId, string message);
    Task<List<GameLog>> GetLatestFive(string gameId);
    Task<List<GameLog>> GetAll (string gameId);
}