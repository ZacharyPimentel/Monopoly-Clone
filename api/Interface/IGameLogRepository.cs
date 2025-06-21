using api.Entity;
namespace api.Interface;
public interface IGameLogRepository : IBaseRepository<GameLog,int>
{
    Task<List<GameLog>> GetLatestFive(Guid gameId);
    Task<List<GameLog>> GetAll(Guid gameId);
}