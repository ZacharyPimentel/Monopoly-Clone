using System.Data;
using api.Entity;
using api.Interface;
namespace api.Repository;
public class PlayerIconRepository(IDbConnection db) : BaseRepository<PlayerIcon, int>(db, "PlayerIcon"), IPlayerIconRepository
{

}