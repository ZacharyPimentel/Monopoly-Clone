
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Entity;
[ExportTsInterface]
public class SocketEventPlayerCreate
{
    public required string PlayerName { get; set; }
    public required int IconId { get; set; }
    public required Guid GameId { get; set; }
}