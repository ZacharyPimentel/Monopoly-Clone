public class PlayerTradeCreateParams
{
    public required string PlayerId { get; set; }
    public bool Initiator { get; set; }
    public int Money { get; set;} = 0;
    public int GetOutOfJailFreeCards { get; set;} = 0;
    public List<int> GamePropertyIds { get; set; } = [];
}

public class PlayerTrade
{
    public int Id { get; set; }
    public int TradeId { get; set; }
    public required string PlayerId { get; set; }
    public bool Initiator {get; set; }
    public int Money { get; set;} = 0;
    public int GetOutOfJailFreeCards { get; set;} = 0;
    //joined fields
    public List<TradeProperty> TradeProperties{ get; set;} = [];
}