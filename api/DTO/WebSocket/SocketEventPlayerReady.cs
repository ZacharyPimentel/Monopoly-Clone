using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventPlayerReady
{
    public required bool IsReadyToPlay { get; set; }
}