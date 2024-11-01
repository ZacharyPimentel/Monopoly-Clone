public class PlayerSearchParams
{
    public bool? Active { get; set; }
}
public class PlayerUpdateParams
{
    public bool? Active {get;set;}
    public string? PlayerName { get; set;}
    public int? IconId { get; set;}

}

public class PlayerCreateParams
{
    public required string PlayerName { get; set;}
    public required int IconId { get; set;}
}

public interface IPlayerRepository
{
    Task<Player> GetByIdAsync(string id);
    Task<IEnumerable<Player>> GetAllAsync();
    Task<IEnumerable<Player>> Search(PlayerSearchParams searchParams);
    Task<bool> Update(string playerId, PlayerUpdateParams updateParams);

    Task<Player> Create(PlayerCreateParams createParams);
}