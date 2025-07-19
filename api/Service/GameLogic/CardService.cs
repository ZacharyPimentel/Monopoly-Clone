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
    public Task BackThreeSpaces(Card card, SpaceLandingServiceContext context);
    public Task GoToJail(Card card, SpaceLandingServiceContext context);
    public Task ReceiveGetOutOfJailFreeCard(Card card, SpaceLandingServiceContext context);
    public Task ReceiveFromPlayers(Card card, SpaceLandingServiceContext context);
    public Task AdvanceToRailroad(SpaceLandingServiceContext context);
    public Task AdvanceToUtility(SpaceLandingServiceContext context);
    public Task PayPlayers(Card card, SpaceLandingServiceContext context);
    public Task PayForHouseUpgrades(Card card, SpaceLandingServiceContext context);
}

public class CardService(
    IPlayerRepository playerRepository,
    IBoardMovementService boardMovementService,
    IJailService jailService,
    IGamePropertyRepository gamePropertyRepository,
    IGameService gameService
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
                await BackThreeSpaces(card, context);
                break;
            case (int)CardActionIds.GoToJail:
                await GoToJail(card, context);
                break;
            case (int)CardActionIds.GetOutOfJailFree:
                await ReceiveGetOutOfJailFreeCard(card, context);
                break;
            case (int)CardActionIds.PayHouseHotel:
                await PayForHouseUpgrades(card, context);
                break;
            case (int)CardActionIds.ReceiveFromPlayers:
                await ReceiveFromPlayers(card, context);
                break;
            case (int)CardActionIds.AdvanceToRailroad:
                await AdvanceToRailroad(context);
                break;
            case (int)CardActionIds.AdvanceToUtility:
                await AdvanceToUtility(context);
                break;
            case (int)CardActionIds.PayPlayers:
                await PayPlayers(card, context);
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
        await boardMovementService.AdvanceToSpaceWithCard(context.CurrentPlayer, card, context.Game);
        var movedToSpace = context.BoardSpaces.First(bs => bs.Id == card.AdvanceToSpaceId);
        string message = $"{context.CurrentPlayer.PlayerName} advanced to {movedToSpace.BoardSpaceName}.";

        message = movedToSpace.Id == 1
            ? $"{context.CurrentPlayer.PlayerName} advanced to {movedToSpace.BoardSpaceName} and collected $200."
            : $"{context.CurrentPlayer.PlayerName} advanced to {movedToSpace.BoardSpaceName}."
        ;
        // await gameLogRepository.CreateAsync(new GameLogCreateParams
        // {
        //     GameId = context.Game.Id,
        //     Message = message
        // });
        //await socketMessageService.SendGamePlayers(context.Game.Id);
    }

    public async Task BackThreeSpaces(Card card, SpaceLandingServiceContext context)
    {
        await boardMovementService.MovePlayerBackThreeSpaces(context.CurrentPlayer);
    }

    public async Task GoToJail(Card card, SpaceLandingServiceContext context)
    {
        await jailService.SendPlayerToJail(context.CurrentPlayer);
    }

    public async Task ReceiveGetOutOfJailFreeCard(Card card, SpaceLandingServiceContext context)
    {
        context.CurrentPlayer.GetOutOfJailFreeCards += 1;
        await playerRepository.UpdateAsync(context.CurrentPlayer.Id, PlayerUpdateParams.FromPlayer(context.CurrentPlayer));

    }
    public async Task ReceiveFromPlayers(Card card, SpaceLandingServiceContext context)
    {
        if (card.Amount is not int receiveAmount)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.CardMissingPaymentAmount);
            throw new Exception(errorMessage);
        }

        int totalPaid = 0;

        //subtract money from other game players
        foreach (var player in context.Players)
        {
            if (player.Id == context.CurrentPlayer.Id) continue;
            await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams
            {
                Money = player.Money - receiveAmount
            });
            totalPaid += receiveAmount;
        }
        //add money from everyone to current player
        await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
        {
            Money = context.CurrentPlayer.Money + totalPaid
        });
    }

    public async Task AdvanceToRailroad(SpaceLandingServiceContext context)
    {
        await boardMovementService.MovePlayerToNearestRailroad(context.CurrentPlayer, context.BoardSpaces);
    }

    public async Task AdvanceToUtility(SpaceLandingServiceContext context)
    {
        await boardMovementService.MovePlayerToNearestUtility(context.CurrentPlayer, context.BoardSpaces);
    }

    public async Task PayPlayers(Card card, SpaceLandingServiceContext context)
    {
        if (card.Amount is not int payAmount)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.CardMissingPaymentAmount);
            throw new Exception(errorMessage);
        }

        int totalToPay = 0;

        //subtract money from other game players
        foreach (var player in context.Players)
        {
            if (player.Id == context.CurrentPlayer.Id) continue;
            await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams
            {
                Money = player.Money + payAmount
            });
            totalToPay += payAmount;
        }
        //add money from everyone to current player
        await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
        {
            Money = context.CurrentPlayer.Money - totalToPay
        });
    }

    public async Task PayForHouseUpgrades(Card card, SpaceLandingServiceContext context)
    {
        int costPerHouse = card.CardTypeId == (int)CardTypeIds.Chance ? 25 : 100;
        int costPerHotel = card.CardTypeId == (int)CardTypeIds.CommunityChest ? 40 : 115;

        IEnumerable<GameProperty> gameProperties = await gamePropertyRepository.SearchAsync(
            new GamePropertyWhereParams { PlayerId = context.CurrentPlayer.Id },
            new { }
        );

        int numberOfHouses = gameProperties
            .Where(gp => gp.UpgradeCount < 5)
            .Sum(gp => gp.UpgradeCount);

        int numberOfHotels = gameProperties
            .Where(gp => gp.UpgradeCount == 5)
            .Count();

        int paymentAmount = (costPerHouse * numberOfHouses) + (costPerHotel * numberOfHotels);

        await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
        {
            Money = context.CurrentPlayer.Money - paymentAmount
        });
        await gameService.CreateGameLog(context.Game.Id, $"{context.CurrentPlayer.PlayerName} paid ${paymentAmount} for their property upgrades.");
    }
}