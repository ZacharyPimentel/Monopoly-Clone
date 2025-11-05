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
    private Trade? FullTrade { get; set; }

    public async Task CreateGameTrade(TradeCreateParams createParams)
    {
        //Only one trade allowed at a time between two people
        var tradingPlayerIds = new List<Guid> {
            createParams.PlayerOne.PlayerId,
            createParams.PlayerTwo.PlayerId
        };
        var playerTrades = await playerTradeRepository.GetActiveByPlayerIds(tradingPlayerIds);
        var existingSharedTradeId = playerTrades
            .GroupBy(pt => pt.TradeId)
            .Where(g => g.Count() > 1)
            .FirstOrDefault();

        if (existingSharedTradeId != null)
        {
            throw new FriendlyException(ErrorType.Error,"Only one trade is allowed between the same two players.");
        }

        await tradeRepository.CreateFullTradeAsync(createParams);
        Player currentPlayer = guardService.GetPlayer();
        IEnumerable<Player> players = guardService.GetPlayers();
        Player otherPlayer = players.First(p => p.Id != currentPlayer.Id);
        await gameService.CreateGameLog(createParams.GameId, $"{currentPlayer.PlayerName} requested a trade with {otherPlayer.PlayerName}.");
        await socketMessageService.SendGameStateUpdate(createParams.GameId, new GameStateIncludeParams
        {
            Trades = true,
            GameLogs = true,
            AudioFile = AudioFiles.TradeUpdated
        });
    }
    public async Task DeclineTrade(Player player, int tradeId)
    {
        FullTrade = await tradeRepository.GetActiveFullTradeAsync(tradeId);
        
        //these guards are copied from ValidateTrade, some of that logic doesn't apply to declining trades.
        if (!FullTrade.PlayerTrades.Any(pt => pt.PlayerId == guardService.GetPlayerId()))
        {
            throw new Exception("You are not allowed to take actions in this trade");
        }

        if (FullTrade.AcceptedBy is not null)
        {
            throw new FriendlyException(ErrorType.Warning,"Cannot complete this action, trade has already been accepted");
        }
        if(FullTrade.DeclinedBy is not null)
        {
            throw new FriendlyException(ErrorType.Warning,"Cannot complete this action, trade has already been declined");
        }
        
        Player otherPlayer = await playerRepository.GetByIdAsync(
            FullTrade.PlayerTrades.Where(pt => pt.PlayerId != player.Id).First().PlayerId
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

        FullTrade = await tradeRepository.GetActiveFullTradeAsync(tradeId);
        await ValidateTrade(playerOneOffer,playerTwoOffer);

        Player currentPlayer = guardService.GetPlayer();
        IEnumerable<Player> players = guardService.GetPlayers();
        Player otherPlayer = players.First(p => p.Id != currentPlayer.Id);

        await tradeRepository.UpdateFullTradeAsync(tradeId, tradeUpdateParams);
        
        await gameService.CreateGameLog(gameId, $"{currentPlayer.PlayerName} updated a trade with {otherPlayer.PlayerName}.");

        await socketMessageService.SendGameStateUpdate(gameId, new GameStateIncludeParams
        {
            Trades = true,
            GameLogs = true,
            AudioFile = AudioFiles.TradeUpdated
        });
    }

    public async Task AcceptTrade(Player player, int tradeId)
    {
        FullTrade = await tradeRepository.GetActiveFullTradeAsync(tradeId);
        await ValidateTrade();

        //not allowed to accept if player was last to update the trade
        if (FullTrade.LastUpdatedBy == player.Id)
        {
            string errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerCannotModifyTrade);
            throw new Exception(errorMessage);
        }

        Player otherPlayer = await playerRepository.GetByIdAsync(
            FullTrade.PlayerTrades.Where(pt => pt.PlayerId != player.Id).First().PlayerId
        );

        //accept trade here
        PlayerTrade currentPlayerTrade = FullTrade.PlayerTrades.First(pt => pt.PlayerId == player.Id);
        PlayerTrade otherPlayerTrade = FullTrade.PlayerTrades.First(pt => pt.PlayerId == otherPlayer.Id);
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
    
    private async Task ValidateTrade(PlayerTradeOffer? playerOneOffer = null, PlayerTradeOffer? playerTwoOffer = null)
    {
        if (FullTrade is null)
        {
            throw new Exception("Full trade should not be null when validating");
        }

        //true if only one condition is true (Exclusive OR)
        if (playerOneOffer is null ^ playerTwoOffer is null)
        {
            throw new Exception("Both trade offers must be provided or neither when validating trade");
        }
        //prevent someone not part of the trade from taking action
        if (!FullTrade.PlayerTrades.Any(pt => pt.PlayerId == guardService.GetPlayerId()))
        {
            throw new Exception("You are not allowed to take actions in this trade");
        }

        if (FullTrade.AcceptedBy is not null)
        {
            throw new FriendlyException(ErrorType.Warning,"Cannot complete this action, trade has already been accepted");
        }
        if(FullTrade.DeclinedBy is not null)
        {
            throw new FriendlyException(ErrorType.Warning,"Cannot complete this action, trade has already been declined");
        }

        Player playerOne;
        Player playerTwo;

        //map PlayerTrades to TradeOffers so same logic can be used for update and accept, decline, etc
        if (playerOneOffer is null)
        {
            playerOne = await playerRepository.GetByIdAsync(FullTrade.PlayerTrades[0].PlayerId);
            playerOneOffer = FullTrade.PlayerTrades.Select(pt => new PlayerTradeOffer
            {
                PlayerId = pt.PlayerId,
                Money = pt.Money,
                GetOutOfJailFreeCards = pt.GetOutOfJailFreeCards,
                GamePropertyIds = [.. pt.TradeProperties.Select(tp => tp.GamePropertyId)]
            }).ToList()[0];
        }
        else
        {
            playerOne = await playerRepository.GetByIdAsync(playerOneOffer.PlayerId);
        }
        if (playerTwoOffer is null)
        {
            playerTwo = await playerRepository.GetByIdAsync(FullTrade.PlayerTrades[1].PlayerId);
            playerTwoOffer = FullTrade.PlayerTrades.Select(pt => new PlayerTradeOffer
            {
                PlayerId = pt.PlayerId,
                Money = pt.Money,
                GetOutOfJailFreeCards = pt.GetOutOfJailFreeCards,
                GamePropertyIds = [.. pt.TradeProperties.Select(tp => tp.GamePropertyId)]
            }).ToList()[1];
        }
        else
        {
            playerTwo = await playerRepository.GetByIdAsync(playerTwoOffer.PlayerId);
        }

        //validate get out of jail free cards
        var offerOneGetOutOfJailFreeValid = playerOneOffer.GetOutOfJailFreeCards <= playerOne.GetOutOfJailFreeCards;
        var offerTwoGetOutOfJailFreeValid = playerTwoOffer.GetOutOfJailFreeCards <= playerTwo.GetOutOfJailFreeCards;
        if(!offerOneGetOutOfJailFreeValid || !offerTwoGetOutOfJailFreeValid)
        {
            throw new FriendlyException(ErrorType.Warning,"Current Trade is invalid - A player in this trade does not have enough get out of jail free cards");
        }
        //validate money
        var offerOneMoneyValid = playerOneOffer.Money <= playerOne.Money;
        var offerTwoMoneyValid = playerTwoOffer.Money <= playerTwo.Money;
        if(!offerOneMoneyValid || !offerTwoMoneyValid)
        {
            throw new FriendlyException(ErrorType.Warning,"Current trade is invalid - A player in this trade does not have enough money.");
        }
        //validate game properties
        var playerOneGameProperties = await gamePropertyRepository.SearchAsync(new GamePropertyWhereParams{ PlayerId = playerOne.Id, UpgradeCount = 0 },new { });
        var playerTwoGameProperties = await gamePropertyRepository.SearchAsync(new GamePropertyWhereParams { PlayerId = playerTwo.Id, UpgradeCount = 0 }, new { });
        var playerOneGamePropertyIds = playerOneGameProperties.Select(gp => gp.Id);
        var playerTwoGamePropertiesIds = playerTwoGameProperties.Select(gp => gp.Id);

        var playerOneGamePropertiesValid =
            !playerOneGameProperties.Any(gp => gp.UpgradeCount > 0) &&
            new HashSet<int>(playerOneGamePropertyIds).IsSupersetOf(playerOneOffer.GamePropertyIds);
        var playerTwoGamePropertiesValid =
            !playerTwoGameProperties.Any(gp => gp.UpgradeCount > 0) &&
            new HashSet<int>(playerTwoGamePropertiesIds).IsSupersetOf(playerTwoOffer.GamePropertyIds);

        if (!playerOneGamePropertiesValid || !playerTwoGamePropertiesValid)
        {
            throw new FriendlyException(ErrorType.Warning,"Current trade is invalid - A property in this trade has been improved or is no longer owned");
        }
    }
}