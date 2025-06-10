namespace api.DTO.Entity;

public class PlayerCreateParams
{

    public required string PlayerName { get; set; }
    public required int IconId { get; set; }
    public required Guid GameId { get; set; }
}