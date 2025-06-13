using api.DTO.Entity;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Interface;
namespace api.Service.GameLogic;

public interface IBoardMovementService
{
    public void MovePlayerWithDiceRoll(Player player, int dieOne, int dieTwo);
    public Task AdvanceToSpaceWithCard(Player player, Card card);
    public Task MovePlayerBackThreeSpaces(Player player);
    public Task MovePlayerToNearestRailroad(Player player, IEnumerable<BoardSpace> boardspaces);
    public Task MovePlayerToNearestUtility(Player player, IEnumerable<BoardSpace> boardspaces);
}

public class BoardMovementService(
    IPlayerRepository playerRepository
) : IBoardMovementService
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

    public async Task AdvanceToSpaceWithCard(Player player, Card card)
    {
        if (card.AdvanceToSpaceId is not int validatedAdvanceToSpaceId)
        {
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.CardMissingAdvanceToSpaceId));
        }

        player.BoardSpaceId = validatedAdvanceToSpaceId;
        bool passedGo = validatedAdvanceToSpaceId <= player.BoardSpaceId;
        if (passedGo) player.Money += 200;
        await playerRepository.UpdateAsync(player.Id, PlayerUpdateParams.FromPlayer(player));
    }

    public async Task MovePlayerBackThreeSpaces(Player player)
    {
        await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams
        {
            BoardSpaceId = player.BoardSpaceId -= 3
        });
    }
    public async Task MovePlayerToNearestRailroad(Player player, IEnumerable<BoardSpace> boardspaces)
    {
        IEnumerable<BoardSpace> railRoads = boardspaces.Where(bs => bs.BoardSpaceCategoryId == (int)BoardSpaceCategories.Railroard);
        BoardSpace? closestRailroad = railRoads.Where(rr => rr.Id > player.BoardSpaceId).FirstOrDefault();
        bool passedGo = false;
        if (closestRailroad != null)
        {
            passedGo = closestRailroad.Id <= player.BoardSpaceId;
            if (passedGo) player.Money += 200;
            player.BoardSpaceId = closestRailroad.Id;
        }
        else
        {
            passedGo = railRoads.First().Id <= player.BoardSpaceId;
            if (passedGo) player.Money += 200;
            player.BoardSpaceId = railRoads.First().Id;
        }

        await playerRepository.UpdateAsync(player.Id, PlayerUpdateParams.FromPlayer(player));
    }

    public async Task MovePlayerToNearestUtility(Player player, IEnumerable<BoardSpace> boardspaces)
    {
        IEnumerable<BoardSpace> utilities = boardspaces.Where(bs => bs.BoardSpaceCategoryId == (int)BoardSpaceCategories.Utility);
        BoardSpace? closestUtility = utilities.Where(rr => rr.Id > player.BoardSpaceId).FirstOrDefault();
        bool passedGo = false;
        if (closestUtility != null)
        {
            passedGo = closestUtility.Id <= player.BoardSpaceId;
            if (passedGo) player.Money += 200;
            player.BoardSpaceId = closestUtility.Id;
        }
        else
        {
            passedGo = utilities.First().Id <= player.BoardSpaceId;
            if (passedGo) player.Money += 200;
            player.BoardSpaceId = utilities.First().Id;
        }

        await playerRepository.UpdateAsync(player.Id, PlayerUpdateParams.FromPlayer(player));
    }

}