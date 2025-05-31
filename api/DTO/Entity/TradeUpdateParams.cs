
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Entity;
[ExportTsInterface]
public class TradeUpdateParams
{
    public PlayerTradeOffer? PlayerOne { get; set; }
    public PlayerTradeOffer? PlayerTwo { get; set; }
    public Guid? LastUpdatedBy { get; set; }
    public Guid? AcceptedBy { get; set; }
    public Guid? DeclinedBy { get; set; }
}