
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventPlayerEdit
{
    public string? PlayerName { get; set; }
    public int? IconId { get; set; }
}