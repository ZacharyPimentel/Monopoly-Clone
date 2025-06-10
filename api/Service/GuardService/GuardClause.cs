using api.Entity;
using api.Service.GuardService;
namespace api.Service.GuardService;

public class GuardClause(Player? player, Game? game) : IGuardClause
{
    private static Player ValidatePlayerExists(Player? player)
    {
        if (player is not Player validatedPlayer) throw new Exception("Can't complete guard check, player doesn't exist");
        return validatedPlayer;
    }
    private static Game ValidateGameExists(Game? game)
    {
        if (game is not Game validatedGame) throw new Exception("Can't complete guard check, game doesn't exist");
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

        if (game.CurrentPlayerTurn != player.Id) throw new Exception("It's not this player's turn");

        return new GuardClause(player, game);
    }
    public IGuardClause PlayerIsInactive()
    {
        Player validatedPlayer = ValidatePlayerExists(player);
        if (validatedPlayer.Active) throw new Exception("This player is active");
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
        if (game.CurrentPlayerTurn != player.Id) throw new Exception("This player is not allowed to roll");
        return new GuardClause(player, game);
    }
}