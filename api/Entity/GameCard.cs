public class GameCard
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public required string GameId { get; set; }

    //joined fields
    public Card? Card { get; set; }
    public string? CardDescription { get; set; }
}