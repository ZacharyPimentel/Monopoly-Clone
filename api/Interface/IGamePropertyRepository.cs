using api.Entity;
namespace api.Interface;

public interface IGamePropertyRepository : IBaseRepository<GameProperty, int>
{
    Task<bool> CreateForNewGameAsync(Guid gameId);
    Task<GameProperty> GetByIdWithDetailsAsync(int GamePropertyId);
    Task<bool> AssignAllToPlayer(Guid gameId, Guid playerId);
}