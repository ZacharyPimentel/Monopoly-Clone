
using TypeGen.Core.TypeAnnotations;

namespace api.Entity;
[ExportTsInterface]
public class Property
{
    public int Id { get; set; }
    public required int PurchasePrice { get; set; }
    public required int MortgageValue { get; set; }
    public required int UpgradeCost { get; set; }
    public required int BoardSpaceId { get; set; }
    public List<PropertyRent> PropertyRents { get; set; } = [];

    //joined GameProperty Id
    public int? GamePropertyId { get; set; }
    public Guid? PlayerId { get; set; }
    public int UpgradeCount { get; set; }
    public bool? Mortgaged { get; set; }
    public Guid? GameId { get; set; }

    //joined From ThemeProperty
    public int? SetNumber { get; set; }
    public string? Color { get; set; }

}