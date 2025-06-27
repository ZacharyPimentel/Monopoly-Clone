using System.Data;
using api.DTO.Entity;
using api.Entity;
using api.Interface;
using Dapper;
namespace api.Repository;
public class TradeRepository(
    IDbConnection db,
    IPlayerTradeRepository playerTradeRepository,
    ITradePropertyRepository tradePropertyRepository
) : BaseRepository<Trade, int>(db, "Trade"), ITradeRepository
{
    public async Task<int> CreateFullTradeAsync(TradeCreateParams createParams)
    {
        Trade trade = await CreateAndReturnAsync(new
        {
            createParams.GameId,
            InitiatedBy = createParams.Initiator,
            LastUpdatedBy = createParams.Initiator
        });

        //create records for first player's trade offer
        var playerTrade1 = await playerTradeRepository.CreateAndReturnAsync(new PlayerTradeCreateParams
        {
            PlayerId = createParams.PlayerOne.PlayerId,
            Money = createParams.PlayerOne.Money,
            GetOutOfJailFreeCards = createParams.PlayerOne.GetOutOfJailFreeCards,
            TradeId = trade.Id
        });
        //create first player property offers
        foreach (int gamePropertyId in createParams.PlayerOne.GamePropertyIds)
        {
            await tradePropertyRepository.CreateAsync(new TradePropertyCreateParams
            {
                GamePropertyId = gamePropertyId,
                PlayerTradeId = playerTrade1.Id
            });
        };

        //create records for second player's trade offer
        var playerTrade2 = await playerTradeRepository.CreateAndReturnAsync(new PlayerTradeCreateParams
        {
            PlayerId = createParams.PlayerTwo.PlayerId,
            Money = createParams.PlayerTwo.Money,
            GetOutOfJailFreeCards = createParams.PlayerTwo.GetOutOfJailFreeCards,
            TradeId = trade.Id
        });
        //create second player property offers
        foreach (int gamePropertyId in createParams.PlayerTwo.GamePropertyIds)
        {
            await tradePropertyRepository.CreateAsync(new TradePropertyCreateParams
            {
                GamePropertyId = gamePropertyId,
                PlayerTradeId = playerTrade2.Id
            });
        };

        return trade.Id;
    }
    public async Task<List<Trade>> GetActiveFullTradesForGameAsync(Guid gameId)
    {
        IEnumerable<Trade> activeTrades = await GetActiveTradesForGameAsync(gameId);

        foreach (var trade in activeTrades)
        {
            IEnumerable<PlayerTrade> playerTrades = await playerTradeRepository.SearchAsync(
                new PlayerTradeWhereParams { TradeId = trade.Id },
                new { }
            );

            foreach (var playerTrade in playerTrades)
            {
                IEnumerable<TradeProperty> tradeProperties =
                    await tradePropertyRepository.GetAllForPlayerTradeWithPropertyDetailsAsync(playerTrade.Id);
                playerTrade.TradeProperties = [.. tradeProperties];
            }

            trade.PlayerTrades = [.. playerTrades];
        }
        return [..activeTrades];
    }
    public async Task<bool> UpdateFullTradeAsync(int tradeId, TradeUpdateParams updateParams)
    {
        await UpdateAsync(tradeId, new TradeUpdateParams
        {
            LastUpdatedBy = updateParams.LastUpdatedBy,
        });

        //loop over each players new offer and update them
        foreach (var offer in new[] { updateParams.PlayerOne, updateParams.PlayerTwo })
        {
            if (offer == null) continue;
            PlayerTrade playerTrade = (await playerTradeRepository.SearchAsync(
                new PlayerTradeWhereParams
                {
                    PlayerId = offer.PlayerId,
                    TradeId = tradeId
                },
                new { }
            )).Single();

            await playerTradeRepository.UpdateAsync(playerTrade.Id, new PlayerTradeUpdateParams
            {
                Money = offer.Money,
                GetOutOfJailFreeCards = offer.GetOutOfJailFreeCards
            });

            await tradePropertyRepository.DeleteManyAsync(
                new TradePropertyWhereParams { PlayerTradeId = playerTrade.Id },
                new { }
            );

            foreach (int gamePropertyId in offer.GamePropertyIds)
            {
                await tradePropertyRepository.CreateAsync(new TradePropertyCreateParams
                {
                    PlayerTradeId = playerTrade.Id,
                    GamePropertyId = gamePropertyId
                });
            }
        }

        return true;
    }
    public async Task<bool> DeclineTrade(int tradeId, Guid playerId)
    {
        var tradeDeclineSql = @"
            UPDATE TRADE
            SET DeclinedBy = @DeclinedBy
            WHERE TradeId = @TradeId
        ";

        await db.ExecuteScalarAsync(tradeDeclineSql, new { tradeId, DeclinedBy = playerId });
        return true;
    }

    public async Task<IEnumerable<Trade>> GetActiveTradesForGameAsync(Guid gameId)
    {
        var sql = @"
            SELECT * 
            FROM Trade
            WHERE DeclinedBy IS NULL
            AND AcceptedBy IS NULL
            AND GameId = @GameId
        ";
        IEnumerable<Trade> trades = await db.QueryAsync<Trade>(sql, new {GameId = gameId});
        return trades;
    }
    public async Task<Trade> GetActiveFullTradeAsync(int tradeId)
    {
        Trade trade = await GetByIdAsync(tradeId);

        IEnumerable<PlayerTrade> playerTrades = await playerTradeRepository.SearchAsync(
            new PlayerTradeWhereParams { TradeId = trade.Id },
            new { }
        );

        foreach (var playerTrade in playerTrades)
        {
            IEnumerable<TradeProperty> tradeProperties =
                await tradePropertyRepository.GetAllForPlayerTradeWithPropertyDetailsAsync(playerTrade.Id);
            playerTrade.TradeProperties = [.. tradeProperties];
        }
        trade.PlayerTrades = [.. playerTrades];
        return trade;
    }
}