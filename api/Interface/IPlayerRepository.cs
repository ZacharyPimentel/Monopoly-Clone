using api.DTO.Entity;
using api.Entity;

namespace api.Interface;
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