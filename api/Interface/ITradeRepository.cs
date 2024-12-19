
public class TradeCreateParams
{
    public required string FromPlayerId;
    public required string ToPlayerId;
    public int? Money;
    public int? GetOutOfJailFreeCards;
    public required string GameId;
    public List<int> GamePropertyIds = [];
}
public class TradeUpdateParams
{
    public required string FromPlayerId;
    public required string ToPlayerId;
    public int? Money;
    public int? GetOutOfJailFreeCards;
    public List<int> GamePropertyIds = [];
}

public interface ITradeRepository
{
    Task<Trade> Create(TradeCreateParams createParams);
    // Task<bool> Delete(int tradeId);
    // Task<Trade> Update(TradeUpdateParams updateParams);
}