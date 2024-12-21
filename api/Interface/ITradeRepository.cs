public class TradeCreateParams
{
    public required PlayerTradeCreateParams PlayerOne { get; set; }
    public required PlayerTradeCreateParams PlayerTwo { get; set;}
    public required string GameId { get; set; }
}

public class TradeUpdateParams
{
    public required PlayerTradeCreateParams PlayerOne { get; set; }
    public required PlayerTradeCreateParams PlayerTwo { get; set;}
    public required int TradeId { get; set; }
}

public interface ITradeRepository
{
    Task<int> Create(TradeCreateParams createParams);
    Task <List<Trade>> Search (string GameId);
    // Task<bool> Delete(int tradeId);
    Task<bool> Update(TradeUpdateParams updateParams);
}