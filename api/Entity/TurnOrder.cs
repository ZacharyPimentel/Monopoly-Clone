public class TurnOrder
{
    public required string Id { get; set;}
    public required string PlayerId { get; set;}
    public required string GameId { get; set;}
    public required int PlayOrder { get; set;}
    public bool HasPlayed { get; set;} = false;
}