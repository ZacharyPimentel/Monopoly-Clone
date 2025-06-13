using api.Entity;
using api.Enumerable;
using api.Helper;
namespace api.Service.GameLogic;

public interface IBoardMovementService
{
    public void MovePlayerWithDiceRoll(Player player, int dieOne, int dieTwo);
    public void MovePlayerWithDrawnCard(Player player, Card card);
}

public class BoardMovementService : IBoardMovementService
{
    public void MovePlayerWithDiceRoll(Player player, int dieOne, int dieTwo)
    {
        //if doubles was rolled 3 times, go straight to jail
        if (player.RollCount + 1 == 3 && dieOne == dieTwo)
        {
            player.InJail = true;
            player.BoardSpaceId = 11;  // 11 is the space for jail
            player.RollCount = 3;
            player.CanRoll = false;
        }

        //move normally otherwise
        int newBoardPosition = player.BoardSpaceId + dieOne + dieTwo;
        bool passedGo = false;
        //handle setting correct position when going over GO
        if (newBoardPosition > 39)
        {
            newBoardPosition %= 40;
            if (newBoardPosition > 0) passedGo = true;
        }
        if (newBoardPosition == 0) newBoardPosition = 1;

        if (passedGo)
        {
            player.Money += 200;
        }

        player.BoardSpaceId = newBoardPosition;
        player.RollCount += player.RollCount + 1;
        if (dieOne != dieTwo)
        {
            player.CanRoll = false;
        }
    }

    public void MovePlayerWithDrawnCard(Player player, Card card)
    {
        if (card.AdvanceToSpaceId is not int validatedAdvanceToSpaceId)
        {
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.CardMissingAdvanceToSpaceId));
        }

        player.BoardSpaceId = validatedAdvanceToSpaceId;
        bool passedGo = validatedAdvanceToSpaceId >= player.BoardSpaceId;
        if (passedGo) player.Money += 200;

    }
}