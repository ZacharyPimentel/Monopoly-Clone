using api.Entity;
using api.Enumerable;
using api.Helper;
namespace api.Service.GuardService;

public class GuardClause(List<Player> players, Player? player, Game? game) : IGuardClause
{
    private static Player ValidatePlayerExists(Player? player)
    {
        if (player is not Player validatedPlayer)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotExist);
            throw new Exception(errorMessage);
        }
        return validatedPlayer;
    }
    private static Game ValidateGameExists(Game? game)
    {
        if (game is not Game validatedGame)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.GameDoesNotExist);
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
        return new GuardClause(players, player, game);
    }
    public IGuardClause GameExists()
    {
        ValidateGameExists(game);
        return new GuardClause(players, player, game);
    }
    public IGuardClause IsCurrentTurn()
    {
        (player, game) = ValidatePlayerAndGameExists(player, game);

        if (game.CurrentPlayerTurn != player.Id)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.NotPlayerTurn);
            throw new Exception(errorMessage);
        }

        return new GuardClause(players, player, game);
    }
    public IGuardClause GameNotStarted()
    {
        Game validatedGame = ValidateGameExists(game);
        if (validatedGame.GameStarted == true)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.GameStarted);
            throw new Exception(errorMessage);
        }
        return new GuardClause(players, player, game);
    }
    public IGuardClause PlayerIsInactive()
    {
        Player validatedPlayer = ValidatePlayerExists(player);
        if (validatedPlayer.Active)
        {
            string errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerActive);
            throw new Exception(errorMessage);
        }
        return new GuardClause(players, player, game);
    }
    public IGuardClause PlayerIsInCorrectGame(Guid? PlayerId = null)
    {
        (player, game) = ValidatePlayerAndGameExists(player, game);
        if (player.GameId != game.Id) throw new Exception("This player is not in the expected game");
        return new GuardClause(players, player, game);
    }
    public IGuardClause PlayerAllowedToRoll()
    {
        (player, game) = ValidatePlayerAndGameExists(player, game);
        if (!player.CanRoll)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerNotAllowedToRoll);
            throw new Exception(errorMessage);
        }
        return new GuardClause(players, player, game);
    }
    public IGuardClause PlayerNotAllowedToRoll()
    {
        (player, game) = ValidatePlayerAndGameExists(player, game);
        if (player.CanRoll)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerAllowedToRoll);
            throw new Exception(errorMessage);
        }
        return new GuardClause(players, player, game);
    }
    public IGuardClause PlayerInJail()
    {
        Player validatedPlayer = ValidatePlayerExists(player);
        if (!validatedPlayer.InJail)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.NotInJail);
            throw new Exception(errorMessage);
        }
        return new GuardClause(players, player, game);
    }

    public IGuardClause PlayerIdInList(IEnumerable<Guid> validIds)
    {
        if (!validIds.Any(id => player?.Id == id))
        {
            var errorMEssage = EnumExtensions.GetEnumDescription(Errors.PlayerIdNotInList);
            throw new Exception(errorMEssage);
        }
        return new GuardClause(players, player, game);
    }

    public IGuardClause PlayersExist()
    {
        foreach (Player player in players)
        {
            ValidatePlayerExists(player);
        }
        return new GuardClause(players, player, game);
    }
    public IGuardClause PlayersAreInCorrectGame()
    {
        foreach (Player player in players)
        {
            PlayerIsInCorrectGame();
        }
        return new GuardClause(players, player, game);
    }
    public IGuardClause PlayerIdsAreInList(IEnumerable<Guid> validIds)
    {
        foreach (Player player in players)
        {
            PlayerIdInList(validIds);
        }
        return new GuardClause(players, player, game);
    }
}