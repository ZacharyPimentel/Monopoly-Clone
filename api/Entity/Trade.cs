using TypeGen.Core.TypeAnnotations;

namespace api.Entity;

[ExportTsInterface]
public class TradeCreateParams
{
    public required Guid Initiator { get; set; }
    public required Guid GameId { get; set; }
    public required PlayerTradeOffer PlayerOne { get; set; }
    public required PlayerTradeOffer PlayerTwo { get; set; }
}
[ExportTsInterface]
public class PlayerTradeOffer
{
    public required Guid PlayerId { get; set; }
    public int Money { get; set; } = 0;
    public int GetOutOfJailFreeCards { get; set; } = 0;
    public List<int> GamePropertyIds { get; set; } = [];
}
[ExportTsInterface]
public class TradeWhereParams
{
    public Guid? GameId { get; set; }
    public Guid? DeclinedBy { get; set; }
    public Guid? AcceptedBy { get; set; }
}
[ExportTsInterface]
public class TradeUpdateParams
{
    public PlayerTradeOffer? PlayerOne { get; set; }
    public PlayerTradeOffer? PlayerTwo { get; set; }
    public Guid? LastUpdatedBy { get; set; }
    public Guid? AcceptedBy { get; set; }
    public Guid? DeclinedBy { get; set; }
}
[ExportTsInterface]
public class Trade
{
    public int Id { get; set; }
    public required Guid GameId { get; set; }
    public required Guid InitiatedBy { get; set; }
    public required Guid LastUpdatedBy { get; set; }
    public Guid? DeclinedBy { get; set; }
    public Guid? AcceptedBy { get; set; }
    //joined fields
    public List<PlayerTrade> PlayerTrades { get; set; } = [];
}
