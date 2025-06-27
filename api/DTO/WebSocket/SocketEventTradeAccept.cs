using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventTradeAccept
{
    public int TradeId { get; set; }
}