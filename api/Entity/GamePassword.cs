namespace api.Entity;

public class GamePassword
{
    public int Id { get; set; }
    public Guid GameId { get; set; }
    public required string Password { get; set; }
}