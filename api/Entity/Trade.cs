using TypeGen.Core.TypeAnnotations;
namespace api.Entity;
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
