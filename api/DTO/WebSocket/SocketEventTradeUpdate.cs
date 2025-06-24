using api.DTO.Entity;
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventTradeUpdate
{
    public required int TradeId { get; set; }
    public required PlayerTradeOffer PlayerOne { get; set; }
    public required PlayerTradeOffer PlayerTwo { get; set; }
}