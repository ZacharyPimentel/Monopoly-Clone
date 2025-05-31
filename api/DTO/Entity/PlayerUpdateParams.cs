using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Entity;
[ExportTsInterface]
public class PlayerUpdateParams
{
    public bool? Active { get; set; }
    public string? PlayerName { get; set; }
    public int? IconId { get; set; }
    public bool? IsReadyToPlay { get; set; }
    public bool? InCurrentGame { get; set; }
    public int? Money { get; set; }
    public int? BoardSpaceId { get; set; }
    public int? RollCount { get; set; }
    public bool? TurnComplete { get; set; }
    public bool? InJail { get; set; }
    public bool? RollingForUtilities { get; set; }
    public int? JailTurnCount { get; set; }
    public int? GetOutOfJailFreeCards { get; set; }
}