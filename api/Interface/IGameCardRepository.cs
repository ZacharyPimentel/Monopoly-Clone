using api.Entity;
using api.Enumerable;

namespace api.Interface;

public interface IGameCardRepository : IBaseRepository<GameCard, int>
{
    public Task<bool> CreateForNewGameAsync(Guid gameId);
    public Task<Card> PullCardForGame(Guid gameId, CardTypeIds cardTypeId);
}