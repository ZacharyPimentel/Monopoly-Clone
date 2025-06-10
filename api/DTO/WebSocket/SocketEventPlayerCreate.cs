
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventPlayerCreate
{
    public required string PlayerName { get; set; }
    public required int IconId { get; set; }
}