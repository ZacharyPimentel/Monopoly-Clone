public class GameLog
{
    public int Id { get; set; }
    public required string GameId { get; set; }
    public required string Message { get; set; }
    public DateTime CreatedAt { get; set; }
}