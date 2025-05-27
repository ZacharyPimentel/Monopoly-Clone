using System.Data;
using api.Interface;

namespace api.Repository;
public class PlayerIconRepository(IDbConnection db) : BaseRepository<PlayerIcon, int>(db, "PlayerIcon"), IPlayerIconRepository
{

}