public class TurnOrder
{
    public required Guid Id { get; set;}
    public required Guid PlayerId { get; set;}
    public required Guid GameId { get; set;}
    public required int PlayOrder { get; set;}
    public bool HasPlayed { get; set;} = false;
}