using System.Data;
using api.Entity;
using api.Interface;
using Dapper;
namespace api.Repository;

public class TradePropertyRepository(IDbConnection db) : BaseRepository<TradeProperty, int>(db, "TradeProperty"), ITradePropertyRepository
{
    public async Task<List<TradeProperty>> GetAllForPlayerTradeWithPropertyDetailsAsync(int playerTradeId)
    {
        var tradePropertySql = @"
            SELECT
                tp.PlayerTradeId,
                tp.GamePropertyId,
                gp.Id,
                gp.PropertyId,
                gp.Mortgaged,
                p.Id,
                p.BoardSpaceId,
                bst.BoardSpaceName AS PropertyName
            FROM
                TradeProperty tp
            LEFT JOIN 
                GameProperty gp ON gp.Id = tp.GamePropertyId
            LEFT JOIN 
                Property p ON p.Id = gp.PropertyId
            LEFT JOIN 
                BoardSpaceTheme bst ON bst.BoardSpaceId = p.BoardSpaceId
            WHERE tp.PlayerTradeId = @PlayerTradeId
        ";
        IEnumerable<TradeProperty> tradeProperties = await db.QueryAsync<TradeProperty>(tradePropertySql, new { PlayerTradeId = playerTradeId });
        return [.. tradeProperties];
    }
}