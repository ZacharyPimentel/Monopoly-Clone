namespace api.Service.GuardService;

public interface IGuardClause
{
    public IGuardClause GameExists();
    public IGuardClause GameNotStarted();
    public IGuardClause IsCurrentTurn();
    public IGuardClause PlayerAllowedToRoll();
    public IGuardClause PlayerExists();
    public IGuardClause PlayerIsInactive();
    public IGuardClause PlayerIsInCorrectGame();
    public IGuardClause PlayerNotAllowedToRoll();
    public IGuardClause PlayerInJail();
}