using System.Data;
using api.Entity;
using api.Interface;

namespace api.Repository;
public class LastDiceRollRepository(IDbConnection db) : BaseRepository<LastDiceRoll, int>(db, "LastDiceRoll"), ILastDiceRollRepository
{

}