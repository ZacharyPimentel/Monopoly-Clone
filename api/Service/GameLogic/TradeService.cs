using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Interface;
using api.Service.GuardService;

namespace api.Service.GameLogic;

public interface ITradeService
{
    public Task CreateGameTrade(TradeCreateParams tradeCreateParams);
    public Task DeclineTrade(Player player, int tradeId);
    public Task UpdateGameTrade(int tradeId, Guid gameId, TradeUpdateParams tradeUpdateParams);
    public Task AcceptTrade(Player player, int tradeId);
}
public class TradeService(
    ITradeRepository tradeRepository,
    IPlayerTradeRepository playerTradeRepository,
    ISocketMessageService socketMessageService,
    IGameService gameService,
    IGuardService guardService,
    IPlayerRepository playerRepository,
    IGamePropertyRepository gamePropertyRepository
) : ITradeService
{
    public async Task CreateGameTrade(TradeCreateParams createParams)
    {
        await tradeRepository.CreateFullTradeAsync(createParams);
        Player currentPlayer = guardService.GetPlayer();
        IEnumerable<Player> players = guardService.GetPlayers();
        Player otherPlayer = players.First(p => p.Id != currentPlayer.Id);
        await gameService.CreateGameLog(createParams.GameId, $"{currentPlayer.PlayerName} requested a trade with {otherPlayer.PlayerName}.");
        await socketMessageService.SendGameStateUpdate(createParams.GameId, new GameStateIncludeParams
        {
            Trades = true,
            GameLogs = true
        });
    }
    public async Task DeclineTrade(Player player, int tradeId)
    {
        IEnumerable<PlayerTrade> playerTrades = await playerTradeRepository.SearchAsync(new PlayerTradeWhereParams
        {
            TradeId = tradeId
        },
        new { }
        );
        if (!playerTrades.Any(pt => pt.PlayerId == player.Id))
        {
            string errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerCannotModifyTrade);
            throw new Exception(errorMessage);
        }

        Player otherPlayer = await playerRepository.GetByIdAsync(
            playerTrades.Where(pt => pt.PlayerId != player.Id).First().PlayerId
        );

        await tradeRepository.UpdateAsync(tradeId, new TradeUpdateParams
        {
            DeclinedBy = player.Id
        });

        Player currentPlayer = guardService.GetPlayer();
        await gameService.CreateGameLog(currentPlayer.GameId, $"{currentPlayer.PlayerName} declined a trade with {otherPlayer.PlayerName}.");

        await socketMessageService.SendGameStateUpdate(player.GameId, new GameStateIncludeParams
        {
            Trades = true,
            GameLogs = true
        });
    }

    public async Task UpdateGameTrade(int tradeId, Guid gameId, TradeUpdateParams tradeUpdateParams)
    {
        if (
            tradeUpdateParams.PlayerOne is not PlayerTradeOffer playerOneOffer ||
            tradeUpdateParams.PlayerTwo is not PlayerTradeOffer playerTwoOffer
        )
        {
            throw new Exception("Player offer is missing");
        }

        Player currentPlayer = guardService.GetPlayer();
        IEnumerable<Player> players = guardService.GetPlayers();
        Player otherPlayer = players.First(p => p.Id != currentPlayer.Id);
        await gameService.CreateGameLog(gameId, $"{currentPlayer.PlayerName} updated a trade with {otherPlayer.PlayerName}.");

        await tradeRepository.UpdateFullTradeAsync(tradeId, tradeUpdateParams);


        await socketMessageService.SendGameStateUpdate(gameId, new GameStateIncludeParams
        {
            Trades = true,
            GameLogs = true
        });
    }
    
    public async Task AcceptTrade(Player player, int tradeId)
    {
        Trade fullTrade = await tradeRepository.GetActiveFullTradeAsync(tradeId);

        //not allowed to accept if player was last to update the trade
        if (fullTrade.LastUpdatedBy == player.Id)
        {
            string errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerCannotModifyTrade);
            throw new Exception(errorMessage);
        }

        //cannot accept if player is not a member of the trade
        if (!fullTrade.PlayerTrades.Any(pt => pt.PlayerId == player.Id))
        {
            string errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerCannotModifyTrade);
            throw new Exception(errorMessage);
        }

        Player otherPlayer = await playerRepository.GetByIdAsync(
            fullTrade.PlayerTrades.Where(pt => pt.PlayerId != player.Id).First().PlayerId
        );

        //accept trade here
        PlayerTrade currentPlayerTrade = fullTrade.PlayerTrades.First(pt => pt.PlayerId == player.Id);
        PlayerTrade otherPlayerTrade = fullTrade.PlayerTrades.First(pt => pt.PlayerId == otherPlayer.Id);
        //update money and get out of jail free cards
        await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams
        {
            GetOutOfJailFreeCards = player.GetOutOfJailFreeCards + otherPlayerTrade.GetOutOfJailFreeCards - currentPlayerTrade.GetOutOfJailFreeCards,
            Money = player.Money + otherPlayerTrade.Money - currentPlayerTrade.Money,
        });
        await playerRepository.UpdateAsync(otherPlayer.Id, new PlayerUpdateParams
        {
            GetOutOfJailFreeCards = otherPlayer.GetOutOfJailFreeCards + currentPlayerTrade.GetOutOfJailFreeCards - otherPlayerTrade.GetOutOfJailFreeCards,
            Money = otherPlayer.Money + currentPlayerTrade.Money - otherPlayerTrade.Money,
        });
        //update property ownership
        foreach (TradeProperty currentPlayerTradeProperty in currentPlayerTrade.TradeProperties)
        {
            await gamePropertyRepository.UpdateAsync(currentPlayerTradeProperty.GamePropertyId, new GamePropertyUpdateParams
            {
                PlayerId = otherPlayer.Id
            });
        }
        foreach (TradeProperty otherPlayerTradeProperty in otherPlayerTrade.TradeProperties)
        {
            await gamePropertyRepository.UpdateAsync(otherPlayerTradeProperty.GamePropertyId, new GamePropertyUpdateParams
            {
                PlayerId = player.Id
            });
        }

        await tradeRepository.UpdateAsync(tradeId, new TradeUpdateParams
        {
            AcceptedBy = player.Id
        });
        await gameService.CreateGameLog(player.GameId, $"{player.PlayerName} accepted a trade with {otherPlayer.PlayerName}.");
        await socketMessageService.SendGameStateUpdate(player.GameId, new GameStateIncludeParams
        {
            Trades = true,
            GameLogs = true,
            BoardSpaces = true,
            Players = true
        });
    }
}