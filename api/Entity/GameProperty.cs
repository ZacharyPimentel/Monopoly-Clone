public class GameProperty
{
    public int Id { get; set; }
    public Guid? PlayerId { get; set; }
    public required Guid GameId { get; set; }
    public required int PropertyId { get; set; }
    public int UpgradeCount { get; set; } = 0;
    public bool Mortgaged {get; set; } = false;
}