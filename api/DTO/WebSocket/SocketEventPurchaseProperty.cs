using api.DTO.Entity;
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventPurchaseProperty
{
    public int GamePropertyId { get; set; }
}