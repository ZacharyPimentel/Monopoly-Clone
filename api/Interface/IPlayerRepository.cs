using api.Interface;

namespace api.Interface;
public class PlayerWhereParams
{
    public bool? Active { get; set; }
    public bool? InCurrentGame { get; set; }
    public Guid? GameId { get; set; }
    public string? ExcludeId { get; set; }
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
    public required Guid GameId { get; set;}
}

public interface IPlayerRepository: IBaseRepository<Player, Guid>
{
    Task<Player> GetByIdWithIconAsync(Guid id);
    Task<IEnumerable<Player>> GetAllWithIconsAsync();
    Task<IEnumerable<Player>> SearchWithIconsAsync(PlayerWhereParams? includeParams = null, PlayerWhereParams? excludeParams = null);
}