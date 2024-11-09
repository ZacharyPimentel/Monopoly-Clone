public class TurnOrder
{
    public int Id { get; set;}
    public required string PlayerId { get; set;}
    public required int GameId { get; set;}
    public required int PlayOrder { get; set;}
    public bool HasPlayed { get; set;} = false;
}