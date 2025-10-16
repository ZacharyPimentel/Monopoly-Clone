using TypeGen.Core.TypeAnnotations;

namespace api.DTO.Entity;

[ExportTsInterface]
public class GameCreateParams
{
    public required string GameName { get; set; }
    public required int ThemeId { get; set; }
}