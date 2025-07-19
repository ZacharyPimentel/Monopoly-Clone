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
    IPlayerRepository playerRepository
) : IPropertyService
{
    public async Task MortgageProperty(int gamePropertyId)
    {
        Player player = guardService.GetPlayer();
        Guid gameId = guardService.SocketConnectionHasGameId().GetGameId();
        GameProperty gameProperty = await gamePropertyRepository.GetByIdWithDetailsAsync(gamePropertyId);
        if (player.Id != gameProperty.PlayerId)
        {
            await gameService.CreateGameLog(gameId, EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotOwnProperty));
        }

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
        if (player.Id != gameProperty.PlayerId)
        {
            await gameService.CreateGameLog(gameId, EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotOwnProperty));
        }

        await gamePropertyRepository.UpdateAsync(gamePropertyId, new GamePropertyUpdateParams
        {
            Mortgaged = false
        });
        //Unmortgaging costs mortgage value + 10%, rounded to nearest integer
        int paymentAmount = (int)Math.Round((gameProperty.MortgageValue ?? 0) * 1.1);
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
        if (player.Id != gameProperty.PlayerId)
        {
            await gameService.CreateGameLog(gameId, EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotOwnProperty));
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotOwnProperty));
        }
        if (gameProperty.UpgradeCount == 5)
        {
            await gameService.CreateGameLog(gameId, EnumExtensions.GetEnumDescription(Errors.PropertyCantBeUpgraded));
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.PropertyCantBeUpgraded));
        }
        
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
        if (player.Id != gameProperty.PlayerId)
        {
            await gameService.CreateGameLog(gameId, EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotOwnProperty));
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotOwnProperty));
        }

        if (gameProperty.UpgradeCount == 0)
        {
            await gameService.CreateGameLog(gameId, EnumExtensions.GetEnumDescription(Errors.PropertyCantBeDowngraded));
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.PropertyCantBeDowngraded));
        }

        await gamePropertyRepository.UpdateAsync(gamePropertyId, new GamePropertyUpdateParams
        {
            UpgradeCount = gameProperty.UpgradeCount - 1,
        });
        int paymentAmount = (int)Math.Round((decimal)(gameProperty.UpgradeCost ?? 0) / 2) ;
        await playerRepository.AddMoneyToPlayer(player.Id, paymentAmount / 2);
        await gameService.CreateGameLog(gameId, $"{player.PlayerName} downgraded {gameProperty.BoardSpaceName} for ${paymentAmount}.");
        await socketMessageService.SendGameStateUpdate(gameId, new GameStateIncludeParams
        {
            BoardSpaces = true,
            Players = true,
            GameLogs = true
        });
    }
}