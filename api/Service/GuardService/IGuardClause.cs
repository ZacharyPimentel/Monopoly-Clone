namespace api.Service.GuardService;

public interface IGuardClause
{
    public IGuardClause GameExists();
    public IGuardClause GameNotStarted();
    public IGuardClause IsCurrentTurn();
    public IGuardClause PlayerAllowedToRoll();
    public IGuardClause PlayerExists();
    public IGuardClause PlayerIsInactive();
    public IGuardClause PlayerIsInCorrectGame(Guid? PlayerId = null);
    public IGuardClause PlayerNotAllowedToRoll();
    public IGuardClause PlayerInJail();
    public IGuardClause PlayerIdInList(IEnumerable<Guid> validIds);
    public IGuardClause PlayersExist();
    public IGuardClause PlayersAreInCorrectGame();
    public IGuardClause PlayerIdsAreInList(IEnumerable<Guid> validIds);
}