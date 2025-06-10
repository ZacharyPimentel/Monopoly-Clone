namespace api.Service.GuardService;

public interface IGuardClause
{
    public IGuardClause IsCurrentTurn();
    public IGuardClause PlayerIsInactive();
    public IGuardClause PlayerIsInCorrectGame();
    public IGuardClause PlayerExists();
    public IGuardClause GameExists();
    public IGuardClause PlayerAllowedToRoll();
    public IGuardClause GameNotStarted();
}