using TypeGen.Core.TypeAnnotations;

namespace api.Entity;

[ExportTsInterface]
public class TradePropertyWhereParams
{
    public int? PlayerTradeId { get; set; }
}
[ExportTsInterface]
public class TradeProperty
{
    public int Id { get; set; }
    public int GamePropertyId { get; set; }
    public int PlayerTradeId { get; set; }
    //joined fields
    public bool Mortgaged { get; set; }
    public required string PropertyName { get; set; }
}