using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Entity;
[ExportTsInterface]
public class TradeWhereParams
{
    public Guid? GameId { get; set; }
    public Guid? DeclinedBy { get; set; }
    public Guid? AcceptedBy { get; set; }
}