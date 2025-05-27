using System.Data;
using Dapper;

namespace api.Repository;
public class TradeRepository(IDbConnection db) : BaseRepository<Trade, int>(db, "Trade"), ITradeRepository
{
    public async Task<int> Create(TradeCreateParams createParams)
    {
        var createTradeSql = @"
                INSERT INTO Trade (GameId,InitiatedBy, LastUpdatedBy)
                VALUES (@GameId, @InitiatedBy, @LastUpdatedBy)
                RETURNING Id
        ";

        var tradeId = await db.ExecuteScalarAsync<int>(createTradeSql, new
        {
            createParams.GameId,
            InitiatedBy = createParams.Initiator,
            LastUpdatedBy = createParams.Initiator
        });

        var createPlayerTradeSql = @"
            INSERT INTO PlayerTrade (TradeId, PlayerId, Money, GetOutOfJailFreeCards)
            VALUES (@TradeId, @PlayerId, @Money, @GetOutOfJailFreeCards)
            RETURNING Id
        ";

        var playerTradeOneId = await db.ExecuteScalarAsync<int>(createPlayerTradeSql, new
        {
            TradeId = tradeId,
            createParams.PlayerOne.PlayerId,
            createParams.PlayerOne.Money,
            createParams.PlayerOne.GetOutOfJailFreeCards
        });
        var playerTradeTwoId = await db.ExecuteScalarAsync<int>(createPlayerTradeSql, new
        {
            TradeId = tradeId,
            createParams.PlayerTwo.PlayerId,
            createParams.PlayerTwo.Money,
            createParams.PlayerTwo.GetOutOfJailFreeCards
        });

        //create first player property offers
        if (createParams.PlayerOne.GamePropertyIds.Count > 0)
        {
            var tradePropertySql = @"
                INSERT INTO TradeProperty (GamePropertyId,PlayerTradeId)
                VALUES (@GamePropertyId,@PlayerTradeId);
            ";

            foreach (int gamePropertyId in createParams.PlayerOne.GamePropertyIds)
            {
                await db.ExecuteAsync(tradePropertySql, new { GamePropertyId = gamePropertyId, PlayerTradeId = playerTradeOneId });
            }
        }

        //create second player property offers
        if (createParams.PlayerOne.GamePropertyIds.Count > 0)
        {
            var tradePropertySql = @"
                INSERT INTO TradeProperty (GamePropertyId,PlayerTradeId)
                VALUES (@GamePropertyId,@PlayerTradeId);
            ";

            foreach (int gamePropertyId in createParams.PlayerTwo.GamePropertyIds)
            {
                await db.ExecuteAsync(tradePropertySql, new { GamePropertyId = gamePropertyId, PlayerTradeId = playerTradeTwoId });
            }
        }

        return tradeId;
    }

    public async Task<List<Trade>> Search(Guid gameId, bool activeOnly = true)
    {
        var tradeSql = "SELECT * FROM Trade WHERE GameId = @GameId";

        var trades = await db.QueryAsync<Trade>(tradeSql, new { GameId = gameId });

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

        //optionally filter out trades that have been declined or accepted (not active)
        if (activeOnly)
        {
            playerTradeSql += " AND pt.DeclinedBy IS NOT NULL AND pt.AcceptedBy IS NOT NULL";
        }

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

        foreach (var trade in trades)
        {
            var playerTrades = await db.QueryAsync<PlayerTrade>(playerTradeSql, new { TradeId = trade.Id });
            trade.PlayerTrades = playerTrades.ToList();

            foreach (var playerTrade in playerTrades)
            {
                var tradeProperties = await db.QueryAsync<TradeProperty>(tradePropertySql, new { PlayerTradeId = playerTrade.Id });
                playerTrade.TradeProperties = tradeProperties.ToList();
            }
        }

        return trades.ToList();
    }
    public async Task<bool> Update(TradeUpdateParams updateParams)
    {
        var playerTradeUpdateSql = @"
            UPDATE PlayerTrade
            SET 
                Money = @Money,
                GetOutOfJailFreeCards = @GetOutOfJailFreeCards,
            WHERE
                TradeId = @TradeId
            AND
                PlayerId = @PlayerId
            RETURNING Id
        ";

        var playerTradeOneId = await db.ExecuteScalarAsync<int>(playerTradeUpdateSql, new
        {
            updateParams.TradeId,
            updateParams.PlayerOne.PlayerId,
            updateParams.PlayerOne.Money,
            updateParams.PlayerOne.GetOutOfJailFreeCards
        });
        var playerTradeTwoId = await db.ExecuteScalarAsync<int>(playerTradeUpdateSql, new
        {
            updateParams.TradeId,
            updateParams.PlayerTwo.PlayerId,
            updateParams.PlayerTwo.Money,
            updateParams.PlayerTwo.GetOutOfJailFreeCards
        });

        var deleteTradePropertySQL = @"
            DELETE FROM TradeProperty
            WHERE 
                PlayerTradeId = @PlayerTradeOneId
            OR 
                PlayerTradeId = @PlayerTradeTwoId
        ";

        //delete trade properties
        await db.ExecuteAsync(deleteTradePropertySQL, new
        {
            PlayerTradeOneId = playerTradeOneId,
            PlayerTradeTwoId = playerTradeTwoId
        });

        //create first player property offers
        if (updateParams.PlayerOne.GamePropertyIds.Count > 0)
        {
            var tradePropertySql = @"
                INSERT INTO TradeProperty (GamePropertyId,PlayerTradeId)
                VALUES (@GamePropertyId,@PlayerTradeId);
            ";

            foreach (int gamePropertyId in updateParams.PlayerOne.GamePropertyIds)
            {
                await db.ExecuteAsync(tradePropertySql, new { GamePropertyId = gamePropertyId, PlayerTradeId = playerTradeOneId });
            }
        }

        //create second player property offers
        if (updateParams.PlayerTwo.GamePropertyIds.Count > 0)
        {
            var tradePropertySql = @"
                INSERT INTO TradeProperty (GamePropertyId,PlayerTradeId)
                VALUES (@GamePropertyId,@PlayerTradeId);
            ";

            foreach (int gamePropertyId in updateParams.PlayerTwo.GamePropertyIds)
            {
                await db.ExecuteAsync(tradePropertySql, new { GamePropertyId = gamePropertyId, PlayerTradeId = playerTradeTwoId });
            }
        }

        //update LastUpdatedBy on the trade
        var tradeUpdateSql = @"
            UPDATE Trade
            SET LastUpdatedBy = @LastUpdatedBy
            WHERE TradeId = @TradeId;
        ";
        await db.ExecuteScalarAsync(tradeUpdateSql, new
        {
            updateParams.LastUpdatedBy,
            updateParams.TradeId
        });

        return true;
    }
    public async Task<bool> DeclineTrade(int tradeId, string playerId)
    {
        var tradeDeclineSql = @"
            UPDATE TRADE
            SET DeclinedBy = @DeclinedBy
            WHERE TradeId = @TradeId
        ";

        await db.ExecuteScalarAsync(tradeDeclineSql, new { tradeId, DeclinedBy = playerId });
        return true;
    }

    public Task<bool> DeclineTrade(int tradeId, Guid playerId)
    {
        throw new NotImplementedException();
    }
}