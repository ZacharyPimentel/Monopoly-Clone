using api.DTO.Entity;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Interface;
namespace api.Service.GameLogic;

public interface IBoardMovementService
{
    public string MovePlayerWithDiceRoll(Player player, Game game, int dieOne, int dieTwo);
    public Task AdvanceToSpaceWithCard(Player player, Card card, Game game);
    public Task MovePlayerBackThreeSpaces(Player player);
    public Task MovePlayerToNearestRailroad(Player player, IEnumerable<BoardSpace> boardspaces);
    public Task MovePlayerToNearestUtility(Player player, IEnumerable<BoardSpace> boardspaces);
    public Task ToggleOffGameMovement(Guid gameId);
    public Task ToggleOnGameMovement(Guid gameId);
}

public class BoardMovementService(
    IPlayerRepository playerRepository,
    ISocketMessageService socketMessageService,
    IGameRepository gameRepository
) : IBoardMovementService
{
    public string MovePlayerWithDiceRoll(Player player, Game game, int dieOne, int dieTwo)
    {
        string message = string.Empty;

        //if doubles was rolled 3 times, go straight to jail
        if (player.RollCount + 1 == 3 && dieOne == dieTwo)
        {
            player.InJail = true;
            player.PreviousBoardSpaceId = player.BoardSpaceId;
            player.BoardSpaceId = 11;  // 11 is the space for jail
            player.RollCount = 3;
            player.CanRoll = false;
            return $"{player.PlayerName} rolled doubles 3 times and went to jail.";
        }

        //move normally otherwise
        int newBoardPosition = player.BoardSpaceId + dieOne + dieTwo;
        bool passedGo = false;
        //handle setting correct position when going over GO
        if (newBoardPosition > 40)
        {
            newBoardPosition %= 40;
            if (newBoardPosition > 1)
            {
                passedGo = true;
                message = $"{player.PlayerName} passed GO and collected $200.";
            }
        }
        if (newBoardPosition == 1)
        {
            newBoardPosition = 1;
        }

        if (passedGo) player.Money += 200;

        player.PreviousBoardSpaceId = player.BoardSpaceId;
        player.BoardSpaceId = newBoardPosition;
        player.RollCount += 1;
        if (dieOne != dieTwo)
        {
            player.CanRoll = false;
        }
        return message;
    }

    public async Task AdvanceToSpaceWithCard(Player player, Card card, Game game)
    {
        if (card.AdvanceToSpaceId is not int validatedAdvanceToSpaceId)
        {
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.CardMissingAdvanceToSpaceId));
        }
        bool passedGo = validatedAdvanceToSpaceId <= player.BoardSpaceId;
        player.PreviousBoardSpaceId = player.BoardSpaceId;
        player.BoardSpaceId = validatedAdvanceToSpaceId;
        if (passedGo)
        {
            player.Money += 200;
            if (validatedAdvanceToSpaceId != 1)
            {
                await socketMessageService.CreateAndSendLatestGameLogs(game.Id, $"{player.PlayerName} passed GO and collected $200.");
            }
        }
        await playerRepository.UpdateAsync(player.Id, PlayerUpdateParams.FromPlayer(player));
    }

    public async Task MovePlayerBackThreeSpaces(Player player)
    {
        await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams
        {
            PreviousBoardSpaceId = player.BoardSpaceId,
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
            player.PreviousBoardSpaceId = player.BoardSpaceId;
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
            player.PreviousBoardSpaceId = player.BoardSpaceId;
            player.BoardSpaceId = closestUtility.Id;
        }
        else
        {
            passedGo = utilities.First().Id <= player.BoardSpaceId;
            if (passedGo) player.Money += 200;
            player.PreviousBoardSpaceId = player.BoardSpaceId;
            player.BoardSpaceId = utilities.First().Id;
        }

        await playerRepository.UpdateAsync(player.Id, PlayerUpdateParams.FromPlayer(player));
    }

    public async Task ToggleOffGameMovement(Guid gameId)
    {
        await gameRepository.UpdateAsync(gameId, new GameUpdateParams
        {
            MovementInProgress = false
        });
    }
    public async Task ToggleOnGameMovement(Guid gameId)
    {
        await gameRepository.UpdateAsync(gameId, new GameUpdateParams
        {
            MovementInProgress = true
        });
    }

}