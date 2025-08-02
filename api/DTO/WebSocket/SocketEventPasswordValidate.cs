using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventPasswordValidate
{
    public Guid GameId { get; set; }
    public required string Password { get; set; }
}