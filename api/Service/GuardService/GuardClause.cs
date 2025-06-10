using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Service.GuardService;
namespace api.Service.GuardService;

public class GuardClause(Player? player, Game? game) : IGuardClause
{
    private static Player ValidatePlayerExists(Player? player)
    {
        if (player is not Player validatedPlayer)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(WebSocketErrors.PlayerDoesNotExist);
            throw new Exception(errorMessage);
        }
        return validatedPlayer;
    }
    private static Game ValidateGameExists(Game? game)
    {
        if (game is not Game validatedGame)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(WebSocketErrors.GameDoesNotExist);
            throw new Exception(errorMessage);
        }
        return validatedGame;
    }
    private static (Player, Game) ValidatePlayerAndGameExists(Player? player, Game? game)
    {
        Player validatedPlayer = ValidatePlayerExists(player);
        Game validatedGame = ValidateGameExists(game);
        return (validatedPlayer, validatedGame);
    }
    public IGuardClause PlayerExists()
    {
        ValidatePlayerExists(player);
        return new GuardClause(player, game);
    }
    public IGuardClause GameExists()
    {
        ValidateGameExists(game);
        return new GuardClause(player, game);
    }
    public IGuardClause IsCurrentTurn()
    {
        (player, game) = ValidatePlayerAndGameExists(player, game);

        if (game.CurrentPlayerTurn != player.Id)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(WebSocketErrors.NotPlayerTurn);
            throw new Exception(errorMessage);
        }

        return new GuardClause(player, game);
    }
    public IGuardClause GameNotStarted()
    {
        Game validatedGame = ValidateGameExists(game);
        if (validatedGame.GameStarted == true)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(WebSocketErrors.GameStarted);
            throw new Exception(errorMessage);
        }
        return new GuardClause(player, game);
    }
    public IGuardClause PlayerIsInactive()
    {
        Player validatedPlayer = ValidatePlayerExists(player);
        if (validatedPlayer.Active)
        {
            string errorMessage = EnumExtensions.GetEnumDescription(WebSocketErrors.PlayerActive);
            throw new Exception(errorMessage);
        }
        return new GuardClause(player, game);
    }
    public IGuardClause PlayerIsInCorrectGame()
    {
        (player, game) = ValidatePlayerAndGameExists(player, game);
        if (player.GameId != game.Id) throw new Exception("This player is not in the expected game");
        return new GuardClause(player, game);
    }
    public IGuardClause PlayerAllowedToRoll()
    {
        (player, game) = ValidatePlayerAndGameExists(player, game);
        if (!player.CanRoll)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(WebSocketErrors.PlayerNotAllowedToRoll);
            throw new Exception(errorMessage);
        }
        return new GuardClause(player, game);
    }
    public IGuardClause PlayerNotAllowedToRoll()
    {
        (player, game) = ValidatePlayerAndGameExists(player, game);
        if (player.CanRoll)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(WebSocketErrors.PlayerAllowedToRoll);
            throw new Exception(errorMessage);
        }
        return new GuardClause(player, game);
    }
}