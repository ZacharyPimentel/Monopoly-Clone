
using System.Runtime.CompilerServices;

public class TradeCreateParams
{
    public required PlayerTradeCreateParams PlayerOne { get; set; }
    public required PlayerTradeCreateParams PlayerTwo { get; set;}
    public required string GameId { get; set; }
}

public interface ITradeRepository
{
    void Create(TradeCreateParams createParams);
    // Task<bool> Delete(int tradeId);
    // Task<Trade> Update(TradeUpdateParams updateParams);
}