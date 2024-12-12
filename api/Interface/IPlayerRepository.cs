public class PlayerWhereParams
{
    public bool? Active { get; set; }
    public bool? InCurrentGame { get; set; }
    public string? GameId { get; set; }
}
public class PlayerUpdateParams
{
    public bool? Active {get;set;}
    public string? PlayerName { get; set;}
    public int? IconId { get; set;}
    public bool? IsReadyToPlay { get; set;}
    public bool? InCurrentGame { get; set;}
    public int? Money { get; set;}
    public int? BoardSpaceId { get; set;}
    public int? RollCount { get; set;}
    public bool? TurnComplete { get; set;}
    public bool? InJail { get; set;}
    public bool? RollingForUtilities { get; set;}
    public int? JailTurnCount { get; set;}
    public int? GetOutOfJailFreeCards { get; set;}
}

public class PlayerCreateParams
{
    public required string PlayerName { get; set;}
    public required int IconId { get; set;}
    public required string GameId { get; set;}
}

public interface IPlayerRepository
{
    Task<Player> GetByIdAsync(string id);
    Task<IEnumerable<Player>> GetAllAsync();
    Task<IEnumerable<Player>> Search(PlayerWhereParams searchParams);
    Task<bool> Update(string playerId, PlayerUpdateParams updateParams);
    Task<bool> UpdateMany(PlayerWhereParams whereParams, PlayerUpdateParams updateParams);
    Task<Player> Create(PlayerCreateParams createParams);
}