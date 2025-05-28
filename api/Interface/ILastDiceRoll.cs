namespace api.Interface;

public class LastDiceRollUpdateParams
{
    public int? DiceOne { get; set; }
    public int? DiceTwo { get; set; }
    public int? UtilityDiceOne { get; set; }
    public int? UtilityDiceTwo { get; set; }
}
public class LastDiceRollWhereParams
{
    public Guid? GameId { get; set; }
}
public interface ILastDiceRollRepository : IBaseRepository<LastDiceRoll, int>
{
}