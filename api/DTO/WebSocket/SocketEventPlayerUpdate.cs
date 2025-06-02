using api.DTO.Entity;
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventPlayerUpdate
{
    public required Guid PlayerId { get; set; }
    public required PlayerUpdateParams PlayerUpdateParams { get; set; }
}