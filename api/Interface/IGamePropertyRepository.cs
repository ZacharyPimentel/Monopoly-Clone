using api.Entity;
namespace api.Interface;

public interface IGamePropertyRepository : IBaseRepository<GameProperty, int>
{
    Task<bool> CreateForNewGameAsync(Guid gameId);
    Task<GameProperty> GetByIdWithDetailsAsync(int GamePropertyId);
    Task<IEnumerable<GameProperty>> GetBySetNumberWithDetails(Guid gameId, int? setNumber);
    Task<IEnumerable<GameProperty>> GetAllWithDetails(Guid gameId);
    Task<bool> AssignAllToPlayer(Guid gameId, Guid playerId);
    Task<bool> UnassignAllFromPlayer(Guid gameId, Guid playerId);
}