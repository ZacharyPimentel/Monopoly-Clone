
using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.hub;
using api.Interface;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
namespace api.Service;

public interface IGameService
{
    Task CreateGame(GameCreateParams gameCreateParams);
    Task EndTurn();
    Task JoinGame(Guid gameId);
    Task LeaveGame(Guid gameId);
    Task UpdateRules(Guid GameId, SocketEventRulesUpdate rulesUpdateParams);
}

public class GameService(
    ISocketMessageService socketMessageService,
    IGameRepository gameRepository,
    ILastDiceRollRepository lastDiceRollRepository,
    IGamePropertyRepository gamePropertyRepository,
    IGameCardRepository gameCardRepository,
    GameState<MonopolyHub> gameState,
    ISocketContextAccessor socketContextAccessor,
    IPlayerRepository playerRepository,
    IGameLogRepository gameLogRepository,
    ITradeRepository tradeRepository,
    IBoardSpaceRepository boardSpaceRepository,
    ITurnOrderRepository turnOrderRepository
) : IGameService
{
    private HubCallerContext SocketContext => socketContextAccessor.RequireContext().Context;
    private IHubContext<MonopolyHub> HubContext => socketContextAccessor.RequireContext().HubContext;
    private SocketPlayer CurrentSocketPlayer => gameState.GetPlayer(SocketContext.ConnectionId);

    public async Task CreateGame(GameCreateParams gameCreateParams)
    {
        IEnumerable<Game> gamesWithName = await gameRepository.SearchAsync(
            new GameWhereParams { GameName = gameCreateParams.GameName },
            new { }
        );

        if (gamesWithName.Any())
        {
            var errorMessage = EnumExtensions.GetEnumDescription(WebSocketErrors.GameNameExists);
            throw new Exception(errorMessage);
        }
        var newGame = await gameRepository.CreateAndReturnAsync(gameCreateParams);
        //populate tables for new game
        await lastDiceRollRepository.CreateAsync(new { GameId = newGame.Id });
        await gamePropertyRepository.CreateForNewGameAsync(newGame.Id);
        await gameCardRepository.CreateForNewGameAsync(newGame.Id);
        await socketMessageService.SendToSelf(WebSocketEvents.GameCreate, newGame.Id); ;
        var games = await gameRepository.GetAllAsync();
        await socketMessageService.SendToAll(WebSocketEvents.GameUpdateAll, games);
    }
    public async Task EndTurn()
    {
        if (CurrentSocketPlayer.PlayerId is not Guid playerId || CurrentSocketPlayer.GameId == null)
        {
            throw new Exception("Socket player is missing data, PlayerId or GameId");
        }

        if (CurrentSocketPlayer.GameId is not Guid gameId)
        {
            throw new Exception("Game Id does not exist");
        }
        Player player = await playerRepository.GetByIdAsync((Guid)CurrentSocketPlayer.PlayerId);
        if (!player.CanRoll)
        {
            throw new Exception("You can't end your turn, it's not finished or it's not your turn.");
        }

        Game? currentGame = await gameRepository.GetByIdWithDetailsAsync(gameId) ?? throw new Exception("Game Doesn't Exist.");

        //keep the visuals of the dice in sync between real rolls and utility rolls
        if (currentGame.UtilityDiceOne != null && currentGame.UtilityDiceTwo != null)
        {
            await lastDiceRollRepository.UpdateWhereAsync(
                new LastDiceRollUpdateParams
                {
                    DiceOne = currentGame.UtilityDiceOne,
                    DiceTwo = currentGame.UtilityDiceTwo,
                },
                new LastDiceRollWhereParams { GameId = currentGame.Id },
                new { }
            );
        }

        await turnOrderRepository.UpdateWhereAsync(
            new TurnOrderUpdateParams { HasPlayed = true },
            new TurnOrderWhereParams
            {
                PlayerId = CurrentSocketPlayer.PlayerId,
                GameId = currentGame.Id,
            },
            new { }
        );

        //check if everyone has taken their turn, reset if so
        var notPlayedCount = (await turnOrderRepository.SearchAsync(new TurnOrderSearchParams
        {
            HasPlayed = false,
            GameId = currentGame.Id
        },
            new { }
        )).Count();

        if (notPlayedCount == 0)
        {
            await turnOrderRepository.UpdateWhereAsync(
                new TurnOrderUpdateParams { HasPlayed = false },
                new TurnOrderWhereParams { GameId = currentGame.Id },
                new { }
            );
            await playerRepository.UpdateWhereAsync(
                new PlayerUpdateParams { RollCount = 0 },
                new PlayerWhereParams { InCurrentGame = true },
                new { }
            );
        }

        await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams { CanRoll = false });

        var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = gameId });
        var updatedGame = await gameRepository.GetByIdWithDetailsAsync(gameId);
        await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, groupPlayers);
        await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, updatedGame);
    }
    public async Task JoinGame(Guid gameId)
    {
        SocketPlayer currentSocketPlayer = gameState.GetPlayer(SocketContext.ConnectionId);
        currentSocketPlayer.GameId = gameId;
        await HubContext.Groups.AddToGroupAsync(SocketContext.ConnectionId, gameId.ToString());
        Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
        if (game == null)
        {
            await socketMessageService.SendToSelf(WebSocketEvents.GameUpdate, game);
            return;
        }
        var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = gameId });
        var latestLogs = await gameLogRepository.GetLatestFive(game.Id);
        var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
        var boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameId);

        await socketMessageService.SendToSelf(WebSocketEvents.GameUpdate, game);
        await socketMessageService.SendToSelf(WebSocketEvents.PlayerUpdate, currentSocketPlayer);
        await socketMessageService.SendToSelf(WebSocketEvents.PlayerUpdateGroup, groupPlayers);
        await socketMessageService.SendToSelf(WebSocketEvents.GameLogUpdate, latestLogs);
        await socketMessageService.SendToSelf(WebSocketEvents.TradeUpdate, trades);
        await socketMessageService.SendToSelf(WebSocketEvents.BoardSpaceUpdate, boardSpaces);
    }
    public async Task LeaveGame(Guid gameId)
    {
        SocketPlayer currentSocketPlayer = gameState.GetPlayer(SocketContext.ConnectionId);
        if (currentSocketPlayer.GameId != null)
        {
            if (currentSocketPlayer.PlayerId is Guid playerId)
            {
                await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams { Active = false });
            }
            var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = currentSocketPlayer.GameId });
            await HubContext.Groups.RemoveFromGroupAsync(SocketContext.ConnectionId, gameId.ToString());
            await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, groupPlayers);
            currentSocketPlayer.GameId = null;
            currentSocketPlayer.PlayerId = null;
            var games = await gameRepository.Search(new GameWhereParams { });
            await socketMessageService.SendToAll(WebSocketEvents.GameUpdateAll, games);
        }
    }
    public async Task UpdateRules(Guid gameId, SocketEventRulesUpdate rulesUpdateParams)
    {
        await gameRepository.UpdateAsync(gameId, new GameUpdateParams
        {
            StartingMoney = rulesUpdateParams.StartingMoney,
            FullSetDoublePropertyRent = rulesUpdateParams.FullSetDoublePropertyRent
        });
        var updatedGame = await gameRepository.GetByIdWithDetailsAsync(gameId);
        await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, updatedGame);
    }

}
