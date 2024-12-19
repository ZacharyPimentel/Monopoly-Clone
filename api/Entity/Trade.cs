public class TradeResponseDTO
{
    public required string FromPlayerId;
    public required string ToPlayerId;
    public int? Money;
    public int? GetOutOfJailFreeCards;
    public List<TradeProperty> TradeProperties = [];
}

public class Trade
{
    public int Id { get; set;}
    public required string FromPlayerId { get; set;}
    public required string ToPlayerId { get; set;}
    public required string GameId { get; set;}
    public int Money { get; set;} = 0;
    public int GetOutOfJailFreeCards { get; set;} = 0;
    //joined fields
    public List<TradeProperty> GameProperties{ get; set;} = [];
}
