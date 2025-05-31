namespace api.DTO.Entity;
public class GameLogCreateParams
{
    public required Guid GameId { get; set; }
    public required string Message { get; set; }
}