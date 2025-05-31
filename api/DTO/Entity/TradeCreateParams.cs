using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Entity;
[ExportTsInterface]
public class TradeCreateParams
{
    public required Guid Initiator { get; set; }
    public required Guid GameId { get; set; }
    public required PlayerTradeOffer PlayerOne { get; set; }
    public required PlayerTradeOffer PlayerTwo { get; set; }
}