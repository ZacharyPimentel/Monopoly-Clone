using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventTradeDecline
{
    public int TradeId { get; set; }
}