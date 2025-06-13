using System.Net.WebSockets;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.hub;
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
    ISocketMessageService socketMessageService
) : IGuardService
{
    private HubCallerContext SocketContext => socketContextAccessor.RequireContext().Context;
    private SocketPlayer CurrentSocketPlayer => gameState.GetPlayer(SocketContext.ConnectionId);
    private Player? Player { get; set; } = null;
    private Game? Game { get; set; } = null;

    public Player GetPlayer()
    {
        if (Player == null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerDoesNotExist);
            socketMessageService.SendToSelf(WebSocketEvents.Error, errorMessage );
            throw new Exception(errorMessage);
        }
        return Player;
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

    public async Task HandleGuardError(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch(Exception error)
        {
            await socketMessageService.SendToSelf(WebSocketEvents.Error, error.Message);
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
        if (CurrentSocketPlayer.PlayerId != null) {
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

        return new GuardClause(Player, Game);
    }

}