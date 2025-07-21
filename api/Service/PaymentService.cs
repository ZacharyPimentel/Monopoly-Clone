using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Hubs;
using api.Interface;
using Microsoft.AspNetCore.SignalR;
namespace api.Service;

public interface IPaymentService
{
    public Task PayPlayer(Player payingPlayer, Guid receivingPlayerId, int amount);
    public Task PayBank(Player payingPlayer, int amount);
    public Task SubtractMoneyFromPlayer(Guid playerId, int amount);
    public Task AddMoneyToPlayer(Guid playerId, int amount);

    /// <summary>
    /// Checks if the player is allowed to collect Free Parking.
    /// Pays out if allowed, and resets the amount in Free Parking.
    /// </summary>
    /// <param name="player">The player that the money will be given to.</param>
    public Task PayOutFreeParkingToPlayer(Player player);
}

public class PaymentService(
    IGameRepository gameRepository,
    IPlayerRepository playerRepository
) : IPaymentService
{
    public async Task PayPlayer(Player payingPlayer, Guid receivingPlayerId, int amount)
    {
        //if the payment would drop the player money below 0
        if (payingPlayer.Money - amount < 0)
        {
            payingPlayer.MoneyNeededForPayment = amount;
        }
        //player has enough money, can pay without extra logic needed
        else
        {
            await AddMoneyToPlayer(
                receivingPlayerId,
                amount
            );
            payingPlayer.Money -= amount;
            payingPlayer.MoneyNeededForPayment = 0;
        }
        await playerRepository.UpdateAsync(payingPlayer.Id, PlayerUpdateParams.FromPlayer(payingPlayer));
    }
    public async Task PayBank(Player payingPlayer, int amount)
    {
        //if the payment would drop the player money below 0
        if (payingPlayer.Money - amount < 0)
        {
            payingPlayer.MoneyNeededForPayment = amount;
        }
        //player has enough money, can pay without extra logic needed
        else
        {
            await gameRepository.AddMoneyToFreeParking(
                payingPlayer.GameId,
                amount
            );
            payingPlayer.Money -= amount;
            payingPlayer.MoneyNeededForPayment = 0;
        }
        await playerRepository.UpdateAsync(payingPlayer.Id, PlayerUpdateParams.FromPlayer(payingPlayer));
    }
    public async Task SubtractMoneyFromPlayer(Guid playerId, int amount)
    {
        await playerRepository.SubtractMoneyFromPlayer(playerId, amount);
    }
    public async Task AddMoneyToPlayer(Guid playerId, int amount)
    {
        await playerRepository.AddMoneyToPlayer(playerId, amount);
    }
    public async Task PayOutFreeParkingToPlayer(Player player)
    {
        if (player.BoardSpaceId != 21)
        {
            throw new Exception("This player is not on Free Parking.");
        }

        Game game = await gameRepository.GetByIdAsync(player.GameId);

        if (!game.CollectMoneyFromFreeParking)
        {
            throw new Exception("Free Parking is not enabled for this game.");
        }

        await gameRepository.PayoutFreeParkingToPlayer(player);
    }
}
