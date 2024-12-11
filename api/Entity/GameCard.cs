public class GameCard
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public int GameId { get; set; }
    public required string CardDescription { get; set; }
}