using api.DTO.Entity;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Interface;
using api.Service.GuardService;

namespace api.Service.GameLogic;

public interface IPropertyService
{
    public Task MortgageProperty(int gamePropertyId);
    public Task UnmortgageProperty(int gamePropertyId);
    public Task UpgradeProperty(int gamePropertyId);
    public Task DowngradeProperty(int gamePropertyId);
}
public class PropertyService(
    IGuardService guardService,
    IGamePropertyRepository gamePropertyRepository,
    IGameService gameService,
    ISocketMessageService socketMessageService,
    IPlayerRepository playerRepository,
    IGameRepository gameRepository

) : IPropertyService
{
    public async Task MortgageProperty(int gamePropertyId)
    {
        Player player = guardService.GetPlayer();
        Guid gameId = guardService.SocketConnectionHasGameId().GetGameId();
        GameProperty gameProperty = await gamePropertyRepository.GetByIdWithDetailsAsync(gamePropertyId);

        await ValidatePlayerOwnsProperty(gameProperty);
        await ValidatePropertyMortgageAllowed(gameProperty);

        await gamePropertyRepository.UpdateAsync(gamePropertyId, new GamePropertyUpdateParams
        {
            Mortgaged = true
        });
        await playerRepository.AddMoneyToPlayer(player.Id, gameProperty.MortgageValue ?? 0);
        await gameService.CreateGameLog(gameId, $"{player.PlayerName} mortgaged {gameProperty.BoardSpaceName} for ${gameProperty.MortgageValue}.");
        await socketMessageService.SendGameStateUpdate(gameId, new GameStateIncludeParams
        {
            BoardSpaces = true,
            Players = true,
            GameLogs = true
        });
    }

    public async Task UnmortgageProperty(int gamePropertyId)
    {
        Player player = guardService.GetPlayer();
        Guid gameId = guardService.SocketConnectionHasGameId().GetGameId();
        GameProperty gameProperty = await gamePropertyRepository.GetByIdWithDetailsAsync(gamePropertyId);

        await ValidatePlayerOwnsProperty(gameProperty);

        //Unmortgaging costs mortgage value + 10%, rounded to nearest integer
        int paymentAmount = (int)Math.Round((gameProperty.MortgageValue ?? 0) * 1.1);

        if(player.Money < paymentAmount)
        {
            throw new Exception("Player does not have enough money to Unmortgage");
        }

        await gamePropertyRepository.UpdateAsync(gamePropertyId, new GamePropertyUpdateParams
        {
            Mortgaged = false
        });

        await playerRepository.SubtractMoneyFromPlayer(player.Id, paymentAmount);
        await gameService.CreateGameLog(gameId, $"{player.PlayerName} unmortgaged {gameProperty.BoardSpaceName} for ${paymentAmount}.");
        await socketMessageService.SendGameStateUpdate(gameId, new GameStateIncludeParams
        {
            BoardSpaces = true,
            Players = true,
            GameLogs = true
        });
    }

    public async Task UpgradeProperty(int gamePropertyId)
    {
        Player player = guardService.GetPlayer();
        Guid gameId = guardService.SocketConnectionHasGameId().GetGameId();
        GameProperty gameProperty = await gamePropertyRepository.GetByIdWithDetailsAsync(gamePropertyId);

        await ValidatePlayerOwnsProperty(gameProperty);
        await ValidatePropertyUpgradeAllowed(gameProperty);
        await ValidatePlayerOwnsSet(gameProperty);

        IEnumerable<GameProperty> setGameProperties = await gamePropertyRepository.GetBySetNumberWithDetails
        (
            gameProperty.GameId,
            gameProperty.SetNumber
        );
        Game game = await gameRepository.GetByIdAsync(gameProperty.GameId);
        if (!game.AllowedToBuildUnevenly)
        {
            await ValidateUpgradeEvenly(gameProperty, setGameProperties);
        }
        await ValidateNoSetMortgaged(setGameProperties);

        int paymentAmount = gameProperty.UpgradeCost ?? 0;

        if (player.Money < paymentAmount)
        {
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.NotEnoughMoney));
        }

        await playerRepository.SubtractMoneyFromPlayer(player.Id, paymentAmount);
        await gamePropertyRepository.UpdateAsync(gamePropertyId, new GamePropertyUpdateParams
        {
            UpgradeCount = gameProperty.UpgradeCount + 1,
        });
        await gameService.CreateGameLog(gameId, $"{player.PlayerName} upgraded {gameProperty.BoardSpaceName} for ${paymentAmount}.");
        await socketMessageService.SendGameStateUpdate(gameId, new GameStateIncludeParams
        {
            BoardSpaces = true,
            Players = true,
            GameLogs = true
        });
    }

    public async Task DowngradeProperty(int gamePropertyId)
    {
        Player player = guardService.GetPlayer();
        Guid gameId = guardService.SocketConnectionHasGameId().GetGameId();
        GameProperty gameProperty = await gamePropertyRepository.GetByIdWithDetailsAsync(gamePropertyId);

        await ValidatePlayerOwnsProperty(gameProperty);
        await ValidatePropertyDowngradeAllowed(gameProperty);
        await ValidatePlayerOwnsSet(gameProperty);
        IEnumerable<GameProperty> setGameProperties = await gamePropertyRepository.GetBySetNumberWithDetails
        (
            gameProperty.GameId,
            gameProperty.SetNumber
        );
        Game game = await gameRepository.GetByIdAsync(gameProperty.GameId);
        if (!game.AllowedToBuildUnevenly)
        {
            await ValidateDowngradeEvenly(gameProperty, setGameProperties);
        }

        await ValidateNoSetMortgaged(setGameProperties);
        
        await gamePropertyRepository.UpdateAsync(gamePropertyId, new GamePropertyUpdateParams
        {
            UpgradeCount = gameProperty.UpgradeCount - 1,
        });
        int paymentAmount = (int)Math.Round((decimal)(gameProperty.UpgradeCost ?? 0) / 2);
        await playerRepository.AddMoneyToPlayer(player.Id, paymentAmount);
        await gameService.CreateGameLog(gameId, $"{player.PlayerName} downgraded {gameProperty.BoardSpaceName} for ${paymentAmount}.");
        await socketMessageService.SendGameStateUpdate(gameId, new GameStateIncludeParams
        {
            BoardSpaces = true,
            Players = true,
            GameLogs = true
        });
    }

    private async Task ValidatePropertyUpgradeAllowed(GameProperty gameProperty)
    {
        if (gameProperty.SetNumber is null || gameProperty.UpgradeCount == 5 || gameProperty.Mortgaged)
        {
            await gameService.CreateGameLog(gameProperty.GameId, "This property is not allowed to be upgraded.");
            throw new Exception("This property is not allowed to be upgraded.");
        }
    }
    private async Task ValidatePropertyDowngradeAllowed(GameProperty gameProperty)
    {
        if (gameProperty.SetNumber is null || gameProperty.UpgradeCount == 0 || gameProperty.Mortgaged)
        {
            await gameService.CreateGameLog(gameProperty.GameId, "This property is not allowed to be downgraded.");
            throw new Exception("This property is not allowed to be downgraded.");
        }
    }
    private async Task ValidatePlayerOwnsProperty(GameProperty gameProperty)
    {
        Player player = guardService.GetPlayer();
        if (player.Id != gameProperty.PlayerId)
        {
            await gameService.CreateGameLog(gameProperty.GameId, EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotOwnProperty));
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotOwnProperty));
        }
    }
    private async Task ValidatePropertyMortgageAllowed(GameProperty gameProperty)
    {
        if (gameProperty.UpgradeCount > 0)
        {
            await gameService.CreateGameLog(gameProperty.GameId, "Property cannot be mortgaged while improved.");
            throw new Exception("Property cannot be mortgaged while improved.");
        }
    }
    private async Task ValidatePlayerOwnsSet(GameProperty gameProperty)
    {
        IEnumerable<GameProperty> setGameProperties = await gamePropertyRepository.GetBySetNumberWithDetails(gameProperty.GameId, gameProperty.SetNumber);
        Player player = guardService.GetPlayer();
        if (setGameProperties.Any(gp => gp.PlayerId != player.Id))
        {
            await gameService.CreateGameLog(gameProperty.GameId, "Player does not own all set properties");
            throw new Exception("Player does not own all set properties");
        }
    }
    private async Task ValidateUpgradeEvenly(GameProperty gameProperty, IEnumerable<GameProperty> setGameProperties)
    {
        var minSetUpgradeCount = setGameProperties.Min(gp => gp.UpgradeCount);
        if(gameProperty.UpgradeCount > minSetUpgradeCount)
        {
            await gameService.CreateGameLog(gameProperty.GameId, "Not allowed to build unevenly.");
            throw new Exception("Not allowed to build unevenly.");
        }
    }
    private async Task ValidateDowngradeEvenly(GameProperty gameProperty, IEnumerable<GameProperty> setGameProperties)
    {
        var maxSetUpgradeCount = setGameProperties.Max(gp => gp.UpgradeCount);
        if (gameProperty.UpgradeCount < maxSetUpgradeCount)
        {
            await gameService.CreateGameLog(gameProperty.GameId, "Not allowed to build unevenly.");
            throw new Exception("Not allowed to build unevenly.");
        }
    }
    private async Task ValidateNoSetMortgaged(IEnumerable<GameProperty> setGameProperties)
    {
        if(setGameProperties.Any(gp => gp.Mortgaged))
        {
            await gameService.CreateGameLog(setGameProperties.ToList()[0].GameId, "Cannot improve when a set property is mortgaged.");
            throw new Exception("Cannot improve when a set property is mortgaged.");
        }
    }
}