using TypeGen.Core.TypeAnnotations;

namespace api.Entity;

[ExportTsInterface]
public class GameProperty
{
    public int Id { get; set; }
    public Guid? PlayerId { get; set; }
    public required Guid GameId { get; set; }
    public required int PropertyId { get; set; }
    public int UpgradeCount { get; set; } = 0;
    public bool Mortgaged { get; set; } = false;
    //Joined from Property
    public int? BoardSpaceId { get; set; }
    public int? PurchasePrice { get; set; }
    public int? MortgageValue { get; set; }
    //Joined from BoardSpaceTheme
    public string? BoardSpaceName { get; set; }
}