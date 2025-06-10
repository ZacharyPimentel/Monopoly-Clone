using api.Entity;
namespace api.Service.GameLogic;

public interface IBoardMovementService
{
    public void MovePlayerWithDiceRoll(Player player, int dieOne, int dieTwo);
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
            player.CanRoll = true;
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
            player.CanRoll = true;
        }
    }
}