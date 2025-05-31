using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Entity;
[ExportTsInterface]
public class PlayerWhereParams
{
    public bool? Active { get; set; }
    public bool? InCurrentGame { get; set; }
    public Guid? GameId { get; set; }
    public string? ExcludeId { get; set; }
}