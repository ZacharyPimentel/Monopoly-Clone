namespace api.Entity;

public class LastDiceRoll
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int DiceOne { get; set; }
    public int DiceTwo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}