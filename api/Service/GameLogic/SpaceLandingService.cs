using System.Reflection.Metadata;
using api.DTO.Entity;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Interface;
using Microsoft.AspNetCore.Mvc;

namespace api.Service.GameLogic;

public class SpaceLandingServiceContext
{
    public required IEnumerable<Player> Players { get; set; }
    public required Game Game { get; set; }
    public required Player CurrentPlayer { get; set; }
    public required IEnumerable<BoardSpace> BoardSpaces { get; set; }
    public required BoardSpace LandedOnSpace { get; set; }

}

public interface ISpaceLandingService
{
    public Task HandleLandedOnSpace(IEnumerable<Player> players, Game game);
    public Task HandleLandedOnProperty(SpaceLandingServiceContext context);
    public Task HandleLandedOnRailroad(SpaceLandingServiceContext context);
    public Task HandleLandedOnGoToJail(SpaceLandingServiceContext context);
    public Task HandleLandedOnPayTaxes(SpaceLandingServiceContext context);
    public Task HandleLandedOnChanceOrCommunityChest(SpaceLandingServiceContext context);
}

public class SpaceLandingService(
    IBoardSpaceRepository boardSpaceRepository,
    IPlayerRepository playerRepository,
    IGameCardRepository gameCardRepository,
    ICardService cardService
) : ISpaceLandingService
{
    public async Task HandleLandedOnSpace(IEnumerable<Player> players, Game game)
    {
        IEnumerable<BoardSpace> boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(game.Id);

        Player currentPlayer = players.First(p => p.Id == game.CurrentPlayerTurn);

        var context = new SpaceLandingServiceContext
        {
            Players = players,
            Game = game,
            CurrentPlayer = currentPlayer,
            LandedOnSpace = boardSpaces.First(bs => bs.Id == currentPlayer.BoardSpaceId),
            BoardSpaces = boardSpaces
        };

        switch (context.LandedOnSpace.BoardSpaceCategoryId)
        {
            case (int)BoardSpaceCategories.Go:
                break;

            case (int)BoardSpaceCategories.Property:
                await HandleLandedOnProperty(context);
                break;

            case (int)BoardSpaceCategories.Railroard:
                await HandleLandedOnRailroad(context);
                break;

            case (int)BoardSpaceCategories.Utility:
                await HandleLandedOnUtility(context);
                break;

            case (int)BoardSpaceCategories.Jail:
                break;

            case (int)BoardSpaceCategories.FreeParking:
                break;

            case (int)BoardSpaceCategories.GoToJail:
                await HandleLandedOnGoToJail(context);
                break;

            case (int)BoardSpaceCategories.PayTaxes:
                await HandleLandedOnPayTaxes(context);
                break;

            case (int)BoardSpaceCategories.Chance:
                await HandleLandedOnChanceOrCommunityChest(context);
                break;

            case (int)BoardSpaceCategories.CommunityChest:
                await HandleLandedOnChanceOrCommunityChest(context);
                break;
        }
        
        
    }

    public async Task HandleLandedOnProperty(SpaceLandingServiceContext context)
    {
        if (context.LandedOnSpace.Property is not Property landedOnProperty)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.NoBoardSpaceProperty);
            throw new Exception(errorMessage);
        }

        //property is owned
        if (context.LandedOnSpace.Property.PlayerId is Guid propertyOwnerId)
        {
            // if current player owns the property, do nothing
            if (propertyOwnerId == context.CurrentPlayer.Id) return;
            //if someone else owns the property, pay them
            else
            {
                //if the property is mortgaged, don't pay so nothing happens
                if (landedOnProperty.Mortgaged is bool mortgaged && mortgaged == true) return;
                //calculate payment amount and pay the owner
                else
                {
                    Player propertyOwner = await playerRepository.GetByIdAsync(propertyOwnerId);
                    PropertyRent rentInfo = landedOnProperty.PropertyRents.First(pr => pr.UpgradeNumber == landedOnProperty.UpgradeCount);
                    await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams { Money = context.CurrentPlayer.Money - rentInfo.Rent });
                    await playerRepository.UpdateAsync(propertyOwnerId, new PlayerUpdateParams { Money = propertyOwner.Money + rentInfo.Rent });
                    return;
                }
            }
        }
        // Nothing needs to happen, front end can handle the next step
        return;
    }

    public async Task HandleLandedOnRailroad(SpaceLandingServiceContext context)
    {
        if (context.LandedOnSpace.Property is not Property landedOnProperty)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.NoBoardSpaceProperty);
            throw new Exception(errorMessage);
        }
        //property is owned
        if (context.LandedOnSpace.Property.PlayerId is Guid railroadOwnerId)
        {
            // if current player owns the railroad, do nothing
            if (railroadOwnerId == context.CurrentPlayer.Id) return;
            //if someone else owns the railroad, pay them
            else
            {
                //if the property is mortgaged, don't pay so nothing happens
                if (landedOnProperty.Mortgaged is bool mortgaged && mortgaged == true) return;
                //calculate payment amount and pay the owner
                else
                {
                    Player propertyOwner = await playerRepository.GetByIdAsync(railroadOwnerId);
                    int numberOfOwnedRailroads = context.BoardSpaces
                        .Select(bs =>
                            bs.BoardSpaceCategoryId == (int)BoardSpaceCategories.Railroard &&
                            bs.Property?.PlayerId == railroadOwnerId
                    ).Count();

                    int paymentAmount = 25 * (int)Math.Pow(2, numberOfOwnedRailroads - 1);

                    await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams { Money = context.CurrentPlayer.Money - paymentAmount });
                    await playerRepository.UpdateAsync(railroadOwnerId, new PlayerUpdateParams { Money = propertyOwner.Money + paymentAmount });

                    return;
                }
            }
        }
        // Nothing needs to happen, front end can handle the next step
        return;
    }

    public async Task HandleLandedOnUtility(SpaceLandingServiceContext context)
    {
        if (context.LandedOnSpace.Property is not Property landedOnUtility)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.NoBoardSpaceProperty);
            throw new Exception(errorMessage);
        }

        //property is owned
        if (context.LandedOnSpace.Property.PlayerId is Guid utilityOwnerId)
        {
            // if current player owns the railroad, do nothing
            if (utilityOwnerId == context.CurrentPlayer.Id) return;
            //if someone else owns the railroad, pay them
            else
            {
                //if the property is mortgaged, don't pay so nothing happens
                if (landedOnUtility.Mortgaged is bool mortgaged && mortgaged == true) return;
                //set up the player to roll for payment
                else
                {
                    await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams { RollingForUtilities = true });
                    return;
                }
            }
        }
        // Nothing needs to happen, front end can handle the next step
        return;
    }

    public async Task HandleLandedOnGoToJail(SpaceLandingServiceContext context)
    {
        await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
        {
            BoardSpaceId = 11, //this is the id for jail
            CanRoll = false
        });
        return;
    }

    public async Task HandleLandedOnPayTaxes(SpaceLandingServiceContext context)
    {

        int paymentAmount;

        if (context.LandedOnSpace.Id == 5) // Income tax
        {
            paymentAmount = (int)Math.Round(context.CurrentPlayer.Money * 0.1);
        }
        else
        { // Luxury tax
            paymentAmount = 75;
        }
        await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
        {
            Money = context.CurrentPlayer.Money - paymentAmount,
        });

        return;
    }

    public async Task HandleLandedOnChanceOrCommunityChest(SpaceLandingServiceContext context)
    {
        Card card;
        if (context.LandedOnSpace.BoardSpaceCategoryId == (int)BoardSpaceCategories.Chance)
        {
            card = await gameCardRepository.PullCardForGame(context.Game.Id, CardTypeIds.Chance);
        }
        else
        {
            card = await gameCardRepository.PullCardForGame(context.Game.Id, CardTypeIds.CommunityChest);
        }

        await cardService.HandlePulledCard(card, context);
    }   
}