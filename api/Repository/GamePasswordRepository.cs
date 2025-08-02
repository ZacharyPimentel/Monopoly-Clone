using System.Data;
using api.Entity;
using api.Interface;

namespace api.Repository;
public interface IGamePasswordRepository:IBaseRepository<GamePassword,int>{}
public class GamePasswordRepository(IDbConnection db) : BaseRepository<GamePassword, int>(db, "GamePassword"), IGamePasswordRepository
{
}