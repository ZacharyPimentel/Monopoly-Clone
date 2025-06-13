using System.Reflection.Metadata;
using api.DTO.Entity;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Interface;

namespace api.Service.GameLogic;

public interface ICardService
{
    public Task HandlePulledCard(Card card, SpaceLandingServiceContext context);
    public Task PayBank(Card card, SpaceLandingServiceContext context);
    public Task ReceiveFromBank(Card card, SpaceLandingServiceContext context);
    public Task AdvanceToSpace(Card card, SpaceLandingServiceContext context);
}

public class CardService(
    IPlayerRepository playerRepository,
    IBoardMovementService boardMovementService
) : ICardService
{
    public async Task HandlePulledCard(Card card, SpaceLandingServiceContext context)
    {
        switch (card.CardActionId)
        {
            case (int)CardActionIds.PayBank:
                await PayBank(card, context);
                break;
            case (int)CardActionIds.ReceiveFrombank:
                await ReceiveFromBank(card, context);
                break;
            case (int)CardActionIds.AdvanceToSpace:
                await AdvanceToSpace(card, context);
                break;
            case (int)CardActionIds.BackThreeSpaces:
                break;
            case (int)CardActionIds.GoToJail:
                break;
            case (int)CardActionIds.GetOutOfJailFree:
                break;
            case (int)CardActionIds.PayHouseHotel:
                break;
            case (int)CardActionIds.ReceiveFromPlayers:
                break;
            case (int)CardActionIds.AdvanceToRailroad:
                break;
            case (int)CardActionIds.AdvanceToUtility:
                break;
            case (int)CardActionIds.PayPlayers:
                break;
        }
    }

    public async Task PayBank(Card card, SpaceLandingServiceContext context)
    {
        if (card.Amount is not int paymentAmount)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.CardMissingPaymentAmount);
            throw new Exception(errorMessage);
        }

        await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
        {
            Money = context.CurrentPlayer.Money - paymentAmount
        });
    }

    public async Task ReceiveFromBank(Card card, SpaceLandingServiceContext context)
    {
        if (card.Amount is not int receiveAmount)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.CardMissingPaymentAmount);
            throw new Exception(errorMessage);
        }

        await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
        {
            Money = context.CurrentPlayer.Money + receiveAmount
        });
    }

    public async Task AdvanceToSpace(Card card, SpaceLandingServiceContext context)
    {
        boardMovementService.MovePlayerWithDrawnCard(context.CurrentPlayer, card);
        await playerRepository.UpdateAsync(context.CurrentPlayer.Id, PlayerUpdateParams.FromPlayer(context.CurrentPlayer));
        
    }
}