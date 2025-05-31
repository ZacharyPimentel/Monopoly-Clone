using api.DTO.Entity;
using api.Entity;
using TypeGen.Core.TypeAnnotations;

namespace api.DTO.Websocket;

[ExportTsInterface]
public class SocketEventTradeUpdate
{
    public required int TradeId { get; set; }
    public required TradeUpdateParams TradeUpdateParams{ get; set; }
}