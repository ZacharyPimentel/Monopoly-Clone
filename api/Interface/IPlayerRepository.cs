using api.DTO.Entity;
using api.Entity;
namespace api.Interface;
public interface IPlayerRepository: IBaseRepository<Player, Guid>
{
    Task<Player> GetByIdWithIconAsync(Guid id);
    Task<IEnumerable<Player>> GetAllWithIconsAsync();
    Task<IEnumerable<Player>> SearchWithIconsAsync(PlayerWhereParams? includeParams = null, PlayerWhereParams? excludeParams = null);
}