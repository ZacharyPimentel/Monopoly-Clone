using api.DTO.Entity;
using api.Entity;
using TypeGen.Core.TypeAnnotations;

namespace api.DTO.Websocket;

[ExportTsInterface]
public class SocketEventTradeUpdateParams
{
    public required int TradeId { get; set; }
    public required TradeUpdateParams TradeUpdateParams{ get; set; }
}