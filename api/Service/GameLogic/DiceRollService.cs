using api.Interface;

namespace api.Service.GameLogic;

public interface IDiceRollService
{
    Task<(int dieOne, int dieTwo)> RollTwoDice();
    Task RecordGameDiceRoll(Guid gameId, int dieOne, int dieTwo);
    Task RecordGameUtilityDiceRoll(Guid gameId, int dieOne, int dieTwo );

}
public class DiceRollService(
    ILastDiceRollRepository lastDiceRollRepository
) : IDiceRollService
{
    private readonly Random random = new();

    public Task<(int dieOne, int dieTwo)> RollTwoDice()
    {
        var result = (random.Next(1, 7), random.Next(1, 7));
        return Task.FromResult(result);
    }
    public async Task RecordGameDiceRoll(Guid gameId, int dieOne, int dieTwo)
    {
        await lastDiceRollRepository.UpdateWhereAsync(
            new LastDiceRollUpdateParams { DiceOne = dieOne, DiceTwo = dieTwo },
            new LastDiceRollWhereParams { GameId = gameId },
            new { }
        );
    }
    public async Task RecordGameUtilityDiceRoll(Guid gameId, int dieOne, int dieTwo)
    {
        await lastDiceRollRepository.UpdateWhereAsync(
            new LastDiceRollUpdateParams { UtilityDiceOne = dieOne, UtilityDiceTwo = dieTwo },
            new LastDiceRollWhereParams { GameId = gameId },
            new { }
        );
    }
}

public class DiceRollResult
{
    public required int DieOne { get; set; }
    public required int DieTwo { get; set; }
    public required bool IsDoubles { get; set; }

}