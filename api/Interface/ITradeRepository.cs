using api.Interface;

public class TradeCreateParams
{
    public required PlayerTradeCreateParams PlayerOne { get; set; }
    public required PlayerTradeCreateParams PlayerTwo { get; set; }
    public required Guid GameId { get; set; }
    public required string Initiator { get; set; }
}

public class TradeUpdateParams
{
    public required PlayerTradeCreateParams PlayerOne { get; set; }
    public required PlayerTradeCreateParams PlayerTwo { get; set; }
    public required int TradeId { get; set; }
    public required string LastUpdatedBy { get; set; }
}

public interface ITradeRepository : IBaseRepository<Trade,int>
{
    Task<int> Create(TradeCreateParams createParams);
    Task<List<Trade>> Search(Guid gameId, bool activeOnly = true);
    Task<bool> Update(TradeUpdateParams updateParams);
    Task<bool> DeclineTrade(int tradeId, Guid playerId);
}