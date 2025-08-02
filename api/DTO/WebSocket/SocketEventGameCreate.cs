using api.DTO.Entity;
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventGameCreate
{
    public required GameCreateParams GameCreateParams { get; set; }
    public string? Password { get; set; }
}