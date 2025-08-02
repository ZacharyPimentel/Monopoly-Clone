using TypeGen.Core.TypeAnnotations;
namespace api.Hubs;

[ExportTsInterface]
public class SocketPlayer
{
    public Guid? PlayerId { get; set; } = null;
    public Guid? GameId { get; set; } = null;
    public required string SocketId { get; set; }
    public bool ValidatedGamePassword { get; set; }
}