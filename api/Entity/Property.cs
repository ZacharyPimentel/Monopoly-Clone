public class Property
{
    public int Id { get; set; }
    public int? PlayerId { get; set; }
    public required int SetId { get; set; }
    public required int PurchasePrice { get; set; }
    public required int MortgageValue { get; set; }
    public required int UpgradeCost { get; set; }
    public required int UpgradeCount { get; set; } = 0;
    public required int BoardSpaceId { get; set; }
}