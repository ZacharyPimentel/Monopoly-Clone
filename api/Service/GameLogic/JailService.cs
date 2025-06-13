using api.DTO.Entity;
using api.Entity;
using api.hub;
using api.Interface;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
using static api.Service.GameLogic.JailService;

namespace api.Service.GameLogic;

public interface IJailService
{
    public void RunJailTurnLogic(Player player, int dieOne, int dieTwo);
    public Task PayOutOfJail();

    public Task SendPlayerToJail(Player player);
}

public class JailService(
    GameState<MonopolyHub> gameState,
    IBoardMovementService boardMovementService,
    ISocketMessageService socketMessageService,
    ISocketContextAccessor socketContextAccessor,
    IPlayerRepository playerRepository
) : IJailService
{
    private HubCallerContext SocketContext => socketContextAccessor.RequireContext().Context;

    private SocketPlayer CurrentSocketPlayer => gameState.GetPlayer(SocketContext.ConnectionId);

    public void RunJailTurnLogic(Player player, int dieOne, int dieTwo)
    {
        if (dieOne == dieTwo)
        {
            player.JailTurnCount = 0;
            player.InJail = false;
            //move the player the amount they rolled
            boardMovementService.MovePlayerWithDiceRoll(player, dieOne, dieTwo);
        }
        else
        {
            //This is player's 3rd roll in jail and should be freed
            if (player.JailTurnCount + 1 == 3)
            {
                player.JailTurnCount = 0;
                player.InJail = false;
            }
            //add one to player jail turn count
            else
            {
                player.JailTurnCount += player.JailTurnCount + 1;
            }
        }
    }
    public async Task PayOutOfJail()
    {
        Guid? playerId = CurrentSocketPlayer.PlayerId;
    }

    public async Task SendPlayerToJail(Player player)
    {
        player.BoardSpaceId = 11; //jail space
        player.CanRoll = false;
        await playerRepository.UpdateAsync(player.Id, PlayerUpdateParams.FromPlayer(player));
    }
}