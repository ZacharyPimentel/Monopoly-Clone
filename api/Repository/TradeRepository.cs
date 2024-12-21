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

        return tradeId;
    }

    public async Task<List<Trade>> Search(string gameId)
    {
        // var tradeSql = @"
        //     SELECT 
        //         t.Id,
        //         t.GameId,
        //         pt.Id,
        //         pt.PlayerId,
        //         pt.Money,
        //         pt.GetOutOfJailFreeCards,
        //         tp.GamePropertyId,
        //         gp.Id,
        //         gp.PropertyId,
        //         gp.Mortgaged,
        //         p.Id,
        //         p.BoardSpaceId,
        //         bst.BoardSpaceName
        //     FROM 
        //         Trade t
        //     LEFT JOIN 
        //         PlayerTrade pt ON pt.TradeId = t.Id
        //     LEFT JOIN 
        //         TradeProperty tp ON tp.PlayerTradeId = pt.Id
        //     LEFT JOIN 
        //         GameProperty gp ON gp.Id = tp.GamePropertyId
        //     LEFT JOIN 
        //         Property p ON p.Id = gp.PropertyId
        //     LEFT JOIN 
        //         BoardSpaceTheme bst ON bst.BoardSpaceId = p.BoardSpaceId
        //     WHERE 
        //         t.GameId = @GameId
        // ";

        // var trade = await db.QueryAsync<Trade, PlayerTrade, TradeProperty, Trade>(
        //     tradeSql, 
        //     (trade,playerTrade,tradeProperty) => {
        //         trade.PlayerTrades.Add(playerTrade);
        //         return trade;
        //     },
        //     new {GameId = gameId},
        //     splitOn:"TradeId"
        // );
        // return trade.ToList();


        var tradeSql = "SELECT * FROM Trade WHERE GameId = @GameId";

        var trades = await db.QueryAsync<Trade>(tradeSql, new {GameId = gameId});

        var playerTradeSql = @"
            SELECT 
                pt.Id,
                pt.PlayerId,
                pt.Money,
                pt.GetOutOfJailFreeCards
            FROM 
                PlayerTrade pt
            WHERE 
                pt.TradeId = @TradeId
        ";

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

        foreach(var trade in trades)
        {
            var playerTrades = await db.QueryAsync<PlayerTrade>(playerTradeSql,new {TradeId = trade.Id});
            trade.PlayerTrades = playerTrades.ToList();
            
            foreach(var playerTrade in playerTrades)
            {
                var tradeProperties = await db.QueryAsync<TradeProperty>(tradePropertySql, new {PlayerTradeId = playerTrade.Id});
                playerTrade.TradeProperties = tradeProperties.ToList();
            }
        }
        
        return trades.ToList();
    }
}