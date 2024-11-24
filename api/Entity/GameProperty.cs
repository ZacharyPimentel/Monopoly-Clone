public class GameProperty
{
    public int Id { get; set; }
    public string? PlayerId { get; set; }
    public required string GameId { get; set; }
    public required int PropertyId { get; set; }
    public int UpgradeCount { get; set; } = 0;
    public bool Mortgaged {get; set; } = false;
}