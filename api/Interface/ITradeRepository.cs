using api.DTO.Entity;
using api.Entity;

namespace api.Interface;
public interface ITradeRepository : IBaseRepository<Trade,int>
{
    Task<int> CreateFullTradeAsync(TradeCreateParams createParams);
    Task<List<Trade>> GetActiveFullTradesForGameAsync(Guid gameId);
    Task<bool> UpdateFullTradeAsync(int tradeId, TradeUpdateParams updateParams);
    Task<bool> DeclineTrade(int tradeId, Guid playerId);
    Task<IEnumerable<Trade>> GetActiveTradesForGameAsync(Guid gameId);
}