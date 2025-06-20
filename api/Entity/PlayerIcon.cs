using TypeGen.Core.TypeAnnotations;
namespace api.Entity;
[ExportTsInterface]
public class PlayerIcon
{
    public int Id { get; set; }
    public required string IconUrl { get; set; }
    public required string IconName { get; set; }
}