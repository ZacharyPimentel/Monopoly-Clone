using System.Data;
using Dapper;

public class TradeRepository(IDbConnection db): ITradeRepository
{
    public async Task<int> Create(TradeCreateParams createParams)
    {
        var createTradeSql = @"
                INSERT INTO Trade (GameId)
                VALUES (@GameId)
                RETURNING Id
        ";
        var tradeId = await db.ExecuteScalarAsync<int>(createTradeSql,new { createParams.GameId } );

        var createPlayerTradeSql = @"
            INSERT INTO PlayerTrade (TradeId,PlayerId,Money,GetOutOfJailFreeCards)
            VALUES (@TradeId, @PlayerId, @Money, @GetOutOfJailFreeCards)
            RETURNING Id
        ";
        

        var playerTradeOneId = await db.ExecuteScalarAsync<int>(createPlayerTradeSql, new {
            TradeId = tradeId,
            createParams.PlayerOne.PlayerId,
            createParams.PlayerOne.Money,
            createParams.PlayerOne.GetOutOfJailFreeCards
        });
        var playerTradeTwoId = await db.ExecuteScalarAsync<int>(createPlayerTradeSql, new {
            TradeId = tradeId,
            createParams.PlayerTwo.PlayerId,
            createParams.PlayerTwo.Money,
            createParams.PlayerTwo.GetOutOfJailFreeCards
        });   

        //create first player property offers
        if(createParams.PlayerOne.GamePropertyIds.Count > 0)
        {
            var tradePropertySql = @"
                INSERT INTO TradeProperty (GamePropertyId,PlayerTradeId)
                VALUES (@GamePropertyId,@PlayerTradeId);
            ";

            foreach( int gamePropertyId in createParams.PlayerOne.GamePropertyIds)
            {
                await db.ExecuteAsync(tradePropertySql, new {GamePropertyId = gamePropertyId, PlayerTradeId = playerTradeOneId});
            }
        }

        //create second player property offers
        if(createParams.PlayerOne.GamePropertyIds.Count > 0)
        {
            var tradePropertySql = @"
                INSERT INTO TradeProperty (GamePropertyId,PlayerTradeId)
                VALUES (@GamePropertyId,@PlayerTradeId);
            ";

            foreach( int gamePropertyId in createParams.PlayerTwo.GamePropertyIds)
            {
                await db.ExecuteAsync(tradePropertySql, new {GamePropertyId = gamePropertyId, PlayerTradeId = playerTradeTwoId});
            }
        }


        // var tradeSql = @"
        //     SELECT 
        //         t.Id, t.FromPlayerId, t.ToPlayerId, t.GameId, t.Money, t.GetOutOfJailFreeCards,
        //         tp.GamePropertyId,
        //         gp.Id, gp.PropertyId,
        //         p.Id, p.BoardSpaceId,
        //         bst.BoardSpaceId, bst.BoardSpaceName, bst.ThemeId
        //     FROM Trade t
        //     LEFT JOIN TradeProperty tp ON tp.TradeId = t.Id
        //     LEFT JOIN GameProperty gp ON gp.Id = tp.GamePropertyId
        //     LEFT JOIN Property p ON p.Id = gp.PropertyId
        //     LEFT JOIN BoardSpaceTheme bst ON bst.BoardSpaceId = p.BoardSpaceId
        //     LEFT JOIN Game g ON g.Id = t.GameId
        //     WHERE 
        //         bst.ThemeId = g.ThemeId
        //     AND
        //     t.Id = @TradeId
        // ";

        // var trade = await db.QuerySingleAsync<Trade>(tradeSql, new {TradeId = tradeId, GameId = createParams.GameId});
        // return trade;

        return tradeId;
    }
}