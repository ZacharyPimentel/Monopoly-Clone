using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Hubs;
using api.Interface;
using api.Repository;
using Microsoft.AspNetCore.SignalR;
namespace api.Service;

public interface IPaymentService
{
    public Task PayPlayer(Player payingPlayer, Player receivingPlayer, int amount);
    public Task PayBank(Player payingPlayer, int amount);
    public Task SubtractMoneyFromPlayer(Guid playerId, int amount);
    public Task AddMoneyToPlayer(Guid playerId, int amount);

    /// <summary>
    /// Checks if the player is allowed to collect Free Parking.
    /// Pays out if allowed, and resets the amount in Free Parking.
    /// </summary>
    /// <param name="player">The player that the money will be given to.</param>
    public Task PayOutFreeParkingToPlayer(Player player);
    public Task PayDebt(Player player);
}

public class PaymentService(
    IGameRepository gameRepository,
    IPlayerRepository playerRepository,
    IPlayerDebtRepository playerDebtRepository,
    IGameLogRepository gameLogRepository,
    ISocketMessageService socketMessageService
) : IPaymentService
{
    public async Task PayPlayer(Player payingPlayer, Player receivingPlayer, int amount)
    {
        //if the payment would drop the player money below 0
        if (payingPlayer.Money - amount < 0)
        {
            await playerDebtRepository.CreateAsync(new PlayerDebtCreateParams
            {
                PlayerId = payingPlayer.Id,
                InDebtTo = receivingPlayer.Id,
                Amount = amount
            });
        }
        //player has enough money, can pay without extra logic needed
        else
        {
            await AddMoneyToPlayer(
                receivingPlayer.Id,
                amount
            );
            payingPlayer.Money -= amount;

            await gameLogRepository.CreateAsync(new GameLogCreateParams
            {
                GameId = payingPlayer.GameId,
                Message = $"{payingPlayer.PlayerName} paid {receivingPlayer.PlayerName} ${amount}"
            });
        }
        await playerRepository.UpdateAsync(payingPlayer.Id, PlayerUpdateParams.FromPlayer(payingPlayer));
    }
    public async Task PayBank(Player payingPlayer, int amount)
    {
        //if the payment would drop the player money below 0
        if (payingPlayer.Money - amount < 0)
        {
            await playerDebtRepository.CreateAsync(new PlayerDebtCreateParams
            {
                PlayerId = payingPlayer.Id,
                Amount = amount
            });
        }
        //player has enough money, can pay without extra logic needed
        else
        {
            await gameRepository.AddMoneyToFreeParking(
                payingPlayer.GameId,
                amount
            );
            payingPlayer.Money -= amount;
            await gameLogRepository.CreateAsync(new GameLogCreateParams
            {
                GameId = payingPlayer.GameId,
                Message = $"{payingPlayer.PlayerName} paid the bank ${amount}."
            });
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
    public async Task PayDebt(Player player)
    {
        PlayerDebt firstPlayerDebt = player.Debts.ToList()[0];

        if (player.Money < firstPlayerDebt.Amount)
        {
            throw new Exception("Not enough Money");
        }

        if (firstPlayerDebt.InDebtTo is Guid receivingPlayerId)
        {
            Player receivingPlayer = await playerRepository.GetByIdAsync(receivingPlayerId);
            await PayPlayer(player, receivingPlayer, firstPlayerDebt.Amount);
        }
        else
        {
            await PayBank(player, firstPlayerDebt.Amount);
        }

        await playerDebtRepository.UpdateAsync(firstPlayerDebt.Id, new PlayerDebtUpdateParams
        {
            DebtPaid = true
        });

        await socketMessageService.SendGameStateUpdate(player.GameId, new GameStateIncludeParams
        {
            Players = true,
            GameLogs = true
        });
    }
}
