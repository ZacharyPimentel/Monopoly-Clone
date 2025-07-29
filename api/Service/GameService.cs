
using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.hub;
using api.Hubs;
using api.Interface;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
namespace api.Service;

public interface IGameService
{
    Task CreateGame(GameCreateParams gameCreateParams);
    Task EndTurn(Player player, Game game);
    Task JoinGame(Guid gameId);
    Task LeaveGame(Guid gameId);
    Task UpdateRules(Guid GameId, SocketEventRulesUpdate rulesUpdateParams);
    Task CreateGameLog(Guid GameId, string message);
    Task AddMoneyToFreeParking(Guid gameId, int amount);
    Task EmptyMoneyFromFreeParking(Guid GameId);
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
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.GameNameExists);
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
    public async Task EndTurn(Player player, Game game)
    {

        //keep the visuals of the dice in sync between real rolls and utility rolls
        if (game.UtilityDiceOne != null && game.UtilityDiceTwo != null)
        {
            await lastDiceRollRepository.UpdateWhereAsync(
                new LastDiceRollUpdateParams
                {
                    DiceOne = game.UtilityDiceOne,
                    DiceTwo = game.UtilityDiceTwo,
                },
                new LastDiceRollWhereParams { GameId = game.Id },
                new { }
            );
        }

        await turnOrderRepository.UpdateWhereAsync(
            new TurnOrderUpdateParams { HasPlayed = true },
            new TurnOrderWhereParams
            {
                PlayerId = player.Id,
                GameId = game.Id,
            },
            new { }
        );

        //check if everyone has taken their turn, reset if so
        var notPlayedCount = await turnOrderRepository.GetNumberOfPlayersWhoHaveNotTakenTheirTurn(game.Id);

        if (notPlayedCount == 0)
        {
            await turnOrderRepository.UpdateWhereAsync(
                new TurnOrderUpdateParams { HasPlayed = false },
                new TurnOrderWhereParams { GameId = game.Id },
                new { }
            );
            await playerRepository.UpdateWhereAsync(
                new PlayerUpdateParams { RollCount = 0 },
                new PlayerWhereParams { InCurrentGame = true },
                new { }
            );
        }
        TurnOrder nextTurn = await turnOrderRepository.GetNextTurnByGameAsync(game.Id);
        await playerRepository.UpdateAsync(nextTurn.PlayerId, new PlayerUpdateParams { CanRoll = true });
        await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams { CanRoll = false, RollCount = 0 });

        await CreateGameLog(game.Id, $"It's {nextTurn.PlayerName}'s turn.");
        await socketMessageService.SendGameStateUpdate(game.Id, new GameStateIncludeParams
        {
            Game = true,
            Players = true,
            GameLogs = true
        });
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
        await socketMessageService.SendToSelf(WebSocketEvents.GameStateUpdate, new GameStateResponse
        {
            Game = game,
            Players = groupPlayers,
            GameLogs = latestLogs,
            Trades = trades,
            BoardSpaces = boardSpaces
        });
        await socketMessageService.SendToSelf(WebSocketEvents.PlayerUpdate, currentSocketPlayer);
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
            FullSetDoublePropertyRent = rulesUpdateParams.FullSetDoublePropertyRent,
            ExtraMoneyForLandingOnGo = rulesUpdateParams.ExtraMoneyForLandingOnGo,
            CollectMoneyFromFreeParking = rulesUpdateParams.CollectMoneyFromFreeParking,
        });
        await socketMessageService.SendGameStateUpdate(gameId, new GameStateIncludeParams
        {
            Game = true
        });
    }

    public async Task CreateGameLog(Guid gameId, string message)
    {
        await gameLogRepository.CreateAsync(new GameLogCreateParams
        {
            GameId = gameId,
            Message = message
        });
    }

    public async Task AddMoneyToFreeParking(Guid gameId, int amount)
    {
        await gameRepository.AddMoneyToFreeParking(gameId, amount);
    }
    public async Task EmptyMoneyFromFreeParking(Guid GameId)
    {
        await gameRepository.UpdateAsync(GameId, new GameUpdateParams
        {
            MoneyInFreeParking = 0
        });
    }
}
