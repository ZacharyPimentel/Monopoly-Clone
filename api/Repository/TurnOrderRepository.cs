using System.Data;
using api.Interface;
using Dapper;

namespace api.Repository;

public class TurnOrderRepository(IDbConnection db) : BaseRepository<TurnOrder, Guid>(db, "TurnOrder"), ITurnOrderRepository
{
    public async Task<TurnOrder> GetNextTurnByGameAsync(Guid gameId)
    {
        var sql = @"
            SELECT 
                *
            FROM 
                TurnOrder
            WHERE 
                HasPlayed = false
            AND
                GameId = @GameId
            ORDER BY PlayOrder
            LIMIT 1
        ";

        TurnOrder result = await db.QueryFirstAsync<TurnOrder>(sql, new { GameId = gameId });
        return result;
    }
}