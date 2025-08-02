namespace api.DTO.Entity;

public class GamePasswordCreateParams
{
    public required Guid GameId { get; set; }
    public required string Password { get; set; }
}