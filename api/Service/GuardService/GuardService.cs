using api.Entity;
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
    IGameRepository gameRepository
) : IGuardService
{
    private HubCallerContext SocketContext => socketContextAccessor.RequireContext().Context;
    private SocketPlayer CurrentSocketPlayer => gameState.GetPlayer(SocketContext.ConnectionId);
    private Player? Player { get; set; } = null;
    private Game? Game { get; set; } = null;

    public Player GetPlayer()
    {
        if (Player == null) throw new Exception("Player doesn't exist");
        return Player;
    }
    public Game GetGame()
    {
        if (Game == null) throw new Exception("Game doesn't exist");
        return Game;
    }
    public IGuardService SocketConnectionHasPlayerId()
    {
        if (CurrentSocketPlayer.PlayerId == null) throw new Exception("Socket connection has no player id");
        return this;
    }
    public IGuardService SocketConnectionDoesNotHavePlayerId()
    {
        if (CurrentSocketPlayer.PlayerId != null) throw new Exception("Socket connection has a player id");
        return this;
    }
    public IGuardService SocketConnectionHasGameId()
    {
        if (CurrentSocketPlayer.GameId == null) throw new Exception("Socket connection has no game id");
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

        return new GuardClause(Player,Game);
    }

}