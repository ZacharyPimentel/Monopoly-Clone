using api.DTO.Entity;
using api.Entity;
namespace api.Interface;

public interface IGameRepository : IBaseRepository<Game, Guid>
{
    Task<Game> GetByIdWithDetailsAsync(Guid id);
    Task<List<Game>> Search(GameWhereParams gameWhereParams);
    Task<List<Game>> GetAllWithPlayerCountAsync();
    Task AddMoneyToFreeParking(Guid gameId, int amount);
    Task PayoutFreeParkingToPlayer(Player player);
}