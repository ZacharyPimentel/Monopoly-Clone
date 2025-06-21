using TypeGen.Core.TypeAnnotations;
namespace api.Entity;
[ExportTsInterface]
public class GameLog
{
    public int Id { get; set; }
    public required Guid GameId { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}