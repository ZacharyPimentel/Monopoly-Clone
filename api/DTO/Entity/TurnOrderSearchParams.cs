namespace api.DTO.Entity;

public class TurnOrderSearchParams
{
    public bool? HasPlayed { get; set; }
    public Guid? GameId { get; set; }
}