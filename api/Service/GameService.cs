
using api.DTO.Entity;
using api.Enumerable;
using api.hub;
using api.Interface;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
namespace api.Service;
public interface IGameService
{
    Task CreateGame(GameCreateParams gameCreateParams);
    Task JoinGame(Guid gameId);
    Task LeaveGame(Guid gameId);
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
    IBoardSpaceRepository boardSpaceRepository
) : IGameService
{
    private HubCallerContext SocketContext => socketContextAccessor.RequireContext().Context;
    private IHubContext<MonopolyHub> HubContext => socketContextAccessor.RequireContext().HubContext;
    public async Task CreateGame(GameCreateParams gameCreateParams)
    {
        var newGame = await gameRepository.CreateAndReturnAsync(gameCreateParams);
        //populate tables for new game
        await lastDiceRollRepository.CreateAsync(new { GameId = newGame.Id });
        await gamePropertyRepository.CreateForNewGameAsync(newGame.Id);
        await gameCardRepository.CreateForNewGameAsync(newGame.Id);
        await socketMessageService.SendToSelf(WebSocketEvents.GameCreate, newGame.Id); ;
        var games = await gameRepository.GetAllAsync();
        await socketMessageService.SendToAll(WebSocketEvents.GameUpdateAll, games);
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
        if (currentSocketPlayer.PlayerId is Guid playerId && currentSocketPlayer.GameId != null)
        {
            await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams { Active = false });
            var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = currentSocketPlayer.GameId });
            await HubContext.Groups.RemoveFromGroupAsync(SocketContext.ConnectionId, gameId.ToString());
            await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, groupPlayers);
            currentSocketPlayer.GameId = null;
            currentSocketPlayer.PlayerId = null;
            var games = await gameRepository.Search(new GameWhereParams { });
            await socketMessageService.SendToAll(WebSocketEvents.GameUpdateAll, games);
        }
    }
}
