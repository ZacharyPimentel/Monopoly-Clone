using TypeGen.Core.TypeAnnotations;

namespace api.Entity;

[ExportTsInterface]
public class PlayerTradeCreateParams
{
    public required Guid PlayerId { get; set; }
    public required int TradeId { get; set; }
    public int Money { get; set; } = 0;
    public int GetOutOfJailFreeCards { get; set; } = 0;
}
[ExportTsInterface]
public class PlayerTradeWhereParams
{
    public int? TradeId { get; set; }
    public Guid? PlayerId { get; set; }
}
[ExportTsInterface]
public class PlayerTradeUpdateParams
{
    public int? Money { get; set; }
    public int? GetOutOfJailFreeCards { get; set; }
}
[ExportTsInterface]public class PlayerTrade
{
    public int Id { get; set; }
    public int TradeId { get; set; }
    public required Guid PlayerId { get; set; }
    public int Money { get; set;} = 0;
    public int GetOutOfJailFreeCards { get; set;} = 0;
    //joined fields
    public List<TradeProperty> TradeProperties{ get; set;} = [];
}