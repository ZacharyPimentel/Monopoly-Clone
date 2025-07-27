using System.Data;
using api.Entity;
using api.Interface;
namespace api.Repository;
public interface IPlayerDebtRepository : IBaseRepository<PlayerDebt, int> 
{
    
}

public class PlayerDebtRepository(IDbConnection db) : BaseRepository<PlayerDebt, int>(db, "PlayerDebt"), IPlayerDebtRepository
{
    
}
