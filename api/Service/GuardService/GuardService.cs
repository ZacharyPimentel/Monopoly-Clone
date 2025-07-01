using System.Net.WebSockets;
using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.hub;
using api.Hubs;
using api.Interface;
using api.Service.GuardService;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
namespace api.Service.GuardService;

public class GuardService(
    GameState<MonopolyHub> gameState,
    ISocketContextAccessor socketContextAccessor,
    IPlayerRepository playerRepository,
    IGameRepository gameRepository,
    ISocketMessageService socketMessageService,
    IErrorLogRepository errorLogRepository
) : IGuardService
{
    private HubCallerContext SocketContext => socketContextAccessor.RequireContext().Context;
    private SocketPlayer CurrentSocketPlayer => gameState.GetPlayer(SocketContext.ConnectionId);
    private Player? Player { get; set; } = null;
    private List<Player> Players { get; set; } = [];
    private Game? Game { get; set; } = null;

    public Player GetPlayer()
    {
        if (Player == null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotExist);
            socketMessageService.SendToSelf(WebSocketEvents.Error, errorMessage);
            throw new Exception(errorMessage);
        }
        return Player;
    }
    public Guid GetPlayerId()
    {
        if (CurrentSocketPlayer.PlayerId is not Guid playerId)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.SocketConnectionMissingPlayerId);
            throw new Exception(errorMessage);
        }
        return playerId;
    }
    public Player GetCurrentPlayerFromList()
    {
        Player? player = Players.FirstOrDefault(p => p.Id == CurrentSocketPlayer?.PlayerId);
        if (player == null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotExist);
            socketMessageService.SendToSelf(WebSocketEvents.Error, errorMessage);
            throw new Exception(errorMessage);
        }
        return player;
    }
    public IEnumerable<Player> GetPlayers()
    {
        return Players;
    }

    public Player GetPlayerFromList(Guid playerId)
    {
        Player? player = Players.FirstOrDefault(p => p.Id == playerId);
        if (player == null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotExist);
            socketMessageService.SendToSelf(WebSocketEvents.Error, errorMessage);
            throw new Exception(errorMessage);
        }
        return player;
    }
    public Game GetGame()
    {
        if (Game == null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.GameDoesNotExist);
            throw new Exception(errorMessage);
        }
        return Game;
    }
    public Guid GetGameId()
    {
        if (CurrentSocketPlayer.GameId is not Guid gameId)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.SocketConnectionMissingGameId);
            throw new Exception(errorMessage);
        }
        return gameId;
    }

    public async Task HandleGuardError(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception error)
        {
            await socketMessageService.SendToSelf(WebSocketEvents.Error, error.Message);
            await errorLogRepository.CreateAsync(new ErrorLogCreateParams
            {
                ErrorMessage = error.Message,
                Source = error.Source,
                StackTrace = error.StackTrace,
                InnerException = error.InnerException
            });
        }
    }
    public IGuardService SocketConnectionHasPlayerId()
    {
        if (CurrentSocketPlayer.PlayerId == null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.SocketConnectionMissingPlayerId);
            throw new Exception(errorMessage);
        }
        return this;
    }
    public IGuardService SocketConnectionDoesNotHavePlayerId()
    {
        if (CurrentSocketPlayer.PlayerId != null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.SocketConnectionHasPlayerId);
            throw new Exception(errorMessage);
        }
        return this;
    }
    public IGuardService SocketConnectionHasGameId()
    {
        if (CurrentSocketPlayer.GameId == null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.SocketConnectionMissingGameId);

            throw new Exception(errorMessage);
        }
        return this;
    }
    public IGuardService SocketConnectionDoesNotHaveGameId()
    {
        if (CurrentSocketPlayer.GameId != null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.SocketConnectionHasGameId);
            throw new Exception(errorMessage);
        }
        return this;
    }

    public async Task<IGuardClause> Init(Guid? playerId, Guid? gameId)
    {

        if (playerId is Guid playerIdGuid)
        {
            Player = await playerRepository.GetByIdWithIconAsync(playerIdGuid);
        }

        if (gameId is Guid gameIdGuid)
        {
            Game = await gameRepository.GetByIdWithDetailsAsync(gameIdGuid);
        }

        return new GuardClause(Players, Player, Game);
    }
    public async Task<IGuardClause> InitMultiple(IEnumerable<Guid> playerIds, Guid? gameId)
    {
        List<Player> result = [];
        foreach (Guid playerId in playerIds)
        {
            Player player = await playerRepository.GetByIdWithIconAsync(playerId);
            if (CurrentSocketPlayer.PlayerId == player.Id)
            {
                Player = player;
            }
            result.Add(player);
        }
        Players = result;

        if (gameId is Guid gameIdGuid)
        {
            Game = await gameRepository.GetByIdWithDetailsAsync(gameIdGuid);
        }
        return new GuardClause(Players, Player, Game);
    }

}