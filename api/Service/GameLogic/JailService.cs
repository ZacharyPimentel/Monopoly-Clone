using System.Net.WebSockets;
using api.DTO.Entity;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.hub;
using api.Interface;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
using static api.Service.GameLogic.JailService;

namespace api.Service.GameLogic;

public interface IJailService
{
    public string RunJailTurnLogic(Player player, Game game, int dieOne, int dieTwo);
    public Task PayOutOfJail(Player player);
    public Task SendPlayerToJail(Player player);
}

public class JailService(
    GameState<MonopolyHub> gameState,
    IBoardMovementService boardMovementService,
    ISocketMessageService socketMessageService,
    ISocketContextAccessor socketContextAccessor,
    IPlayerRepository playerRepository,
    IGameLogRepository gameLogRepository
) : IJailService
{
    private HubCallerContext SocketContext => socketContextAccessor.RequireContext().Context;

    private SocketPlayer CurrentSocketPlayer => gameState.GetPlayer(SocketContext.ConnectionId);

    public string RunJailTurnLogic(Player player, Game game, int dieOne, int dieTwo)
    {
        var jailMessage = "";
        if (dieOne == dieTwo)
        {
            player.JailTurnCount = 0;
            player.InJail = false;

            jailMessage = $"{player.PlayerName} is free from jail.";
            //move the player the amount they rolled
            boardMovementService.MovePlayerWithDiceRoll(player, game, dieOne, dieTwo);
        }
        else
        {
            //This is player's 3rd roll in jail and should be freed
            if (player.JailTurnCount + 1 == 3)
            {
                player.JailTurnCount = 0;
                player.InJail = false;
                player.CanRoll = false;
                player.RollCount += 1;
                jailMessage = $"{player.PlayerName} is free from jail.";

            }
            //add one to player jail turn count
            else
            {
                player.JailTurnCount += 1;
                player.CanRoll = false;
                player.RollCount += 1;
                jailMessage = $"{player.PlayerName} is still in jail ({player.JailTurnCount}/3).";
            }
        }
        return jailMessage;
    }
    public async Task PayOutOfJail(Player player)
    {
        if (player.Money < 50)
        {
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.NotEnoughMoney));
        }

        await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams
        {
            Money = player.Money -= 50
        });
        await gameLogRepository.CreateAsync(new GameLogCreateParams
        {
            GameId = player.GameId,
            Message = $"{player.PlayerName} paid $50 to get out of jail."
        });
        await socketMessageService.SendGamePlayers(player.GameId);
    }

    public async Task SendPlayerToJail(Player player)
    {
        player.BoardSpaceId = 11; //jail space
        player.CanRoll = false;
        player.InJail = true;
        await playerRepository.UpdateAsync(player.Id, PlayerUpdateParams.FromPlayer(player));
    }
}