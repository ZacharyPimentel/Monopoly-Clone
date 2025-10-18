using System.Data;
using api.Entity;
using api.Interface;
using Dapper;

namespace api.Repository;
public class LastDiceRollRepository(IDbConnection db) : BaseRepository<LastDiceRoll, int>(db, "LastDiceRoll"), ILastDiceRollRepository
{
    public async Task ResetUtilityDice(Guid gameId)
    {
        var sql = @"
            UPDATE
                LastDiceRoll
            SET
                UtilityDiceOne = NULL,
                UtilityDiceTwo = NULL
            WHERE
                GameId = @GameId
        ";

        await db.ExecuteAsync(sql, new { GameId = gameId });
    }
}