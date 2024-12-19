using System.Data;
using Dapper;

public class TradeRepository(IDbConnection db): ITradeRepository
{
    public async Task<Trade> Create(TradeCreateParams createParams)
    {
        var createSql = @"
                INSERT INTO Trade (FromPlayerId,ToPlayerId,Money,GetOutOfJailFreeCards)
                VALUES (@FromPlayerId,@ToPlayerId,@Money,@GetOutOfJailFreeCards)
                RETURNING Id   
        ";
        var tradeId = await db.ExecuteAsync(createSql,createParams);

        if(createParams.GamePropertyIds.Count > 0)
        {
            var tradePropertySql = @"
                INSERT INTO TradeProperty (GamePropertyId,TradeId)
                VALUES (@GamePropertyId,@TradeId);
            ";

            foreach( int gamePropertyId in createParams.GamePropertyIds)
            {
                await db.ExecuteAsync(tradePropertySql, new {GamePropertyId = gamePropertyId, TradeId = tradeId});
            }
        }

        var tradeSql = @"
            SELECT 
                t.Id, t.FromPlayerId, t.ToPlayerId, t.GameId, t.Money, t.GetOutOfJailFreeCards,
                tp.GamePropertyId,
                gp.Id, gp.PropertyId,
                p.Id, p.BoardSpaceId,
                bst.BoardSpaceId, bst.BoardSpaceName, bst.ThemeId
            FROM Trade t
            LEFT JOIN TradeProperty tp ON tp.TradeId = t.Id
            LEFT JOIN GameProperty gp ON gp.Id = tp.GamePropertyId
            LEFT JOIN Property p ON p.Id = gp.PropertyId
            LEFT JOIN BoardSpaceTheme bst ON bst.BoardSpaceId = p.BoardSpaceId
            LEFT JOIN Game g ON g.Id = t.GameId
            WHERE 
                bst.ThemeId = g.ThemeId
            AND
            t.Id = @TradeId
        ";

        var trade = await db.QuerySingleAsync<Trade>(tradeSql, new {TradeId = tradeId, GameId = createParams.GameId});
        return trade;
    }
}