using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventPlayerReady
{
    public required Guid PlayerId { get; set; }
    public required bool IsReadyToPlay { get; set; }
}