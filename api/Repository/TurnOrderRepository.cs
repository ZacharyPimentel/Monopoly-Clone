using System.Data;
using api.Entity;
using api.Interface;
using Dapper;

namespace api.Repository;

public class TurnOrderRepository(IDbConnection db) : BaseRepository<TurnOrder, Guid>(db, "TurnOrder"), ITurnOrderRepository
{
    public async Task<TurnOrder> GetNextTurnByGameAsync(Guid gameId)
    {
        var sql = @"
            SELECT 
                t.Id,
                t.PlayerId,
                t.GameId,
                t.PlayOrder,
                t.HasPlayed,
                p.Id,
                p.PlayerName
            FROM 
                TurnOrder t
            INNER JOIN Player p ON p.Id = t.PlayerId
            WHERE 
                t.HasPlayed = false
            AND
                t.GameId = @GameId
            ORDER BY t.PlayOrder
            LIMIT 1
        ";

        TurnOrder result = await db.QueryFirstAsync<TurnOrder>(sql, new { GameId = gameId });
        return result;
    }
}