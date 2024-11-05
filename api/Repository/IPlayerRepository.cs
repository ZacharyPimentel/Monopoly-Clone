public class PlayerWhereParams
{
    public bool? Active { get; set; }
}
public class PlayerUpdateParams
{
    public bool? Active {get;set;}
    public string? PlayerName { get; set;}
    public int? IconId { get; set;}
    public bool? IsReadyToPlay { get; set;}
    public bool? InCurrentGame { get; set;}

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
    Task<IEnumerable<Player>> Search(PlayerWhereParams searchParams);
    Task<bool> Update(string playerId, PlayerUpdateParams updateParams);
    Task<bool> UpdateMany(PlayerWhereParams whereParams, PlayerUpdateParams updateParams);
    Task<Player> Create(PlayerCreateParams createParams);
}