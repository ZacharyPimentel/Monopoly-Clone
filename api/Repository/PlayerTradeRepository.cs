using System.Data;
using api.Entity;
using api.Interface;
using api.Repository;
using Dapper;
namespace api.Repository;
public class PlayerTradeRepository(IDbConnection db) : BaseRepository<PlayerTrade, int>(db,"PlayerTrade"), IPlayerTradeRepository
{
    public async Task<IEnumerable<PlayerTrade>> GetActiveByPlayerIds(IEnumerable<Guid>playerIds)
    {
        var sql = @"
            SELECT 
                pt.Id, 
                pt.PlayerId,
                t.DeclinedBy,
                t.AcceptedBy
            FROM PlayerTrade pt
            JOIN Trade t ON t.Id = pt.TradeId
            WHERE PlayerId = ANY(@PlayerIds)
            AND t.DeclinedBy = NULL
            AND t.AcceptedBy = NULL
        ";

        var playerTrades = await db.QueryAsync<PlayerTrade>(sql, new { PlayerIds = playerIds });
        return playerTrades;
    }

}