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
                p.PlayerName,
                p.Bankrupt
            FROM 
                TurnOrder t
            INNER JOIN Player p ON p.Id = t.PlayerId
            WHERE 
                t.HasPlayed = false
            AND
                t.GameId = @GameId
            AND
                p.Bankrupt = false
            ORDER BY t.PlayOrder
            LIMIT 1
        ";

        TurnOrder result = await db.QueryFirstAsync<TurnOrder>(sql, new { GameId = gameId });
        return result;
    }

    public async Task<int> GetNumberOfPlayersWhoHaveNotTakenTheirTurn(Guid gameId)
    {
        var sql = @"
            SELECT 
                t.Id,
                t.HasPlayed,
                p.Id,
                p.Bankrupt
            FROM 
                TurnOrder t
            INNER JOIN Player p on p.Id = t.PlayerId
            WHERE
                t.GameId = @GameId
            AND
                t.HasPlayed = false
            AND
                p.Bankrupt = false
        ";

        IEnumerable<TurnOrder> result = await db.QueryAsync<TurnOrder>(sql, new { GameId = gameId });
        return result.Count();
    }

}