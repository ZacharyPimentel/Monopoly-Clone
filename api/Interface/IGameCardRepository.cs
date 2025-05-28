using api.Entity;

namespace api.Interface;

public interface IGameCardRepository : IBaseRepository<GameCard, int>
{
    public Task<bool> CreateForNewGameAsync(Guid gameId);
}