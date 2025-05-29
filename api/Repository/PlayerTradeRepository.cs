using System.Data;
using api.Entity;
using api.Interface;
using api.Repository;
namespace api.Repository;
public class PlayerTradeRepository(IDbConnection db) : BaseRepository<PlayerTrade, int>(db,"PlayerTrade"), IPlayerTradeRepository
{
    
}