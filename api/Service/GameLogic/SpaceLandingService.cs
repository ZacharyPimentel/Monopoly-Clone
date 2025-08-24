using System.ComponentModel;
using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Interface;

namespace api.Service.GameLogic;

public class SpaceLandingServiceContext
{
    public required IEnumerable<Player> Players { get; set; }
    public required Game Game { get; set; }
    public required Player CurrentPlayer { get; set; }
    public required IEnumerable<BoardSpace> BoardSpaces { get; set; }
    public required BoardSpace LandedOnSpace { get; set; }
    public bool CameFromCard { get; set; }
}

public interface ISpaceLandingService
{
    public Task HandleLandedOnGo(SpaceLandingServiceContext context);
    public Task HandleLandedOnFreeParking(SpaceLandingServiceContext context);
    public Task HandleLandedOnSpace(IEnumerable<Player> players, Game game, bool cameFromCard = false);
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
    ICardService cardService,
    IJailService jailService,
    IGameLogRepository gameLogRepository,
    ISocketMessageService socketMessageService,
    IBoardMovementService boardMovementService,
    IGameService gameService,
    IGameRepository gameRepository,
    IPaymentService paymentService
) : ISpaceLandingService
{
    public async Task HandleLandedOnSpace(IEnumerable<Player> players, Game game, bool cameFromCard = false)
    {
        IEnumerable<BoardSpace> boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(game.Id);

        Player currentPlayer = players.First(p => p.Id == game.CurrentPlayerTurn);

        var context = new SpaceLandingServiceContext
        {
            Players = players,
            Game = game,
            CurrentPlayer = currentPlayer,
            LandedOnSpace = boardSpaces.First(bs => bs.Id == currentPlayer.BoardSpaceId),
            BoardSpaces = boardSpaces,
            CameFromCard = cameFromCard
        };

        switch (context.LandedOnSpace.BoardSpaceCategoryId)
        {
            case (int)BoardSpaceCategories.Go:
                await HandleLandedOnGo(context);
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
                await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                {
                    Game = true,
                    Players = true
                });
                break;

            case (int)BoardSpaceCategories.FreeParking:
                await HandleLandedOnFreeParking(context);
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

    public async Task HandleLandedOnGo(SpaceLandingServiceContext context)
    {
        await boardMovementService.ToggleOffGameMovement(context.Game.Id);
        GameStateIncludeParams updateParams = new()
        {
            Game = true
        };

        if (context.Game.ExtraMoneyForLandingOnGo)
        {
            await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
            {
                Money = context.CurrentPlayer.Money + 300
            });
            updateParams.Players = true;
            await gameService.CreateGameLog(context.Game.Id, $"{context.CurrentPlayer.PlayerName} landed on GO and collected $300.");
        }
        else
        {
            await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
            {
                Money = context.CurrentPlayer.Money + 200
            });
            await gameService.CreateGameLog(context.Game.Id, $"{context.CurrentPlayer.PlayerName} landed on GO and collected $200");
        }
        await socketMessageService.SendGameStateUpdate(context.Game.Id, updateParams);
    }

    public async Task HandleLandedOnFreeParking(SpaceLandingServiceContext context)
    {
        GameStateIncludeParams includeParams = new()
        {
            Game = true,
            GameLogs = true
        };

        await boardMovementService.ToggleOffGameMovement(context.Game.Id);
        await gameService.CreateGameLog(context.Game.Id, $"{context.CurrentPlayer.PlayerName} landed on Free Parking");

        if (context.Game.CollectMoneyFromFreeParking)
        {
            await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams
            {
                Money = context.CurrentPlayer.Money + context.Game.MoneyInFreeParking
            });
            await gameService.EmptyMoneyFromFreeParking(context.Game.Id);

            await gameService.CreateGameLog(context.Game.Id, $"{context.CurrentPlayer.PlayerName} collected ${context.Game.MoneyInFreeParking}.");
            includeParams.Players = true;
        }

        await socketMessageService.SendGameStateUpdate(context.Game.Id, includeParams);
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
            if (propertyOwnerId == context.CurrentPlayer.Id)
            {
                await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                {
                    Game = true,
                    GameLogs = true
                });
                return;
            }
            //if someone else owns the property, pay them
            else
            {
                Player propertyOwner = await playerRepository.GetByIdAsync(propertyOwnerId);
                //if the property is mortgaged, don't pay so nothing happens
                if (landedOnProperty.Mortgaged is bool mortgaged && mortgaged == true)
                {
                    await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                    await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                    {
                        Game = true,
                        GameLogs = true
                    });
                    return;
                }
                //calculate payment amount and pay the owner
                else
                {
                    PropertyRent rentInfo = landedOnProperty.PropertyRents.First(pr => pr.UpgradeNumber == landedOnProperty.UpgradeCount);
                    int paymentAmount = rentInfo.Rent;
                    //if owner has the full set, pay 2x if the game setting is enabled
                    if (context.Game.FullSetDoublePropertyRent)
                    {
                        IEnumerable<BoardSpace> setSpaces = context.BoardSpaces.Where(bs => bs.Property?.SetNumber == landedOnProperty.SetNumber);
                        if (setSpaces.All(bs => bs?.Property?.PlayerId == propertyOwnerId))
                        {
                            paymentAmount *= 2;
                        }
                    }
                    await paymentService.PayPlayer(context.CurrentPlayer, propertyOwner, paymentAmount);
                    await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                    await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                    {
                        Game = true,
                        Players = true,
                        GameLogs = true
                    });
                    return;
                }
            }
        }
        // Nothing needs to happen, front end can handle the next step
        await boardMovementService.ToggleOffGameMovement(context.Game.Id);
        await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
        {
            Game = true,
            GameLogs = true
        });
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
            if (railroadOwnerId == context.CurrentPlayer.Id)
            {
                await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                {
                    Game = true,
                    GameLogs = true
                });
                return;
            }
            //if someone else owns the railroad, pay them
            else
            {
                //if the property is mortgaged, don't pay so nothing happens
                if (landedOnProperty.Mortgaged is bool mortgaged && mortgaged == true)
                {
                    await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                    await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                    {
                        Game = true,
                        GameLogs = true
                    });
                    return;
                }
                //calculate payment amount and pay the owner
                else
                {
                    Player propertyOwner = await playerRepository.GetByIdAsync(railroadOwnerId);
                    int numberOfOwnedRailroads = context.BoardSpaces.Count(bs =>
                        bs.BoardSpaceCategoryId == (int)BoardSpaceCategories.Railroard &&
                        bs.Property?.PlayerId == railroadOwnerId
                    );

                    int paymentAmount = 25 * (int)Math.Pow(2, numberOfOwnedRailroads - 1);
                    if (context.CameFromCard) paymentAmount *= 2; //card says you pay double if owned
                    await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams { Money = context.CurrentPlayer.Money - paymentAmount });
                    await playerRepository.UpdateAsync(railroadOwnerId, new PlayerUpdateParams { Money = propertyOwner.Money + paymentAmount });
                    await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                    await gameLogRepository.CreateAsync(new GameLogCreateParams
                    {
                        GameId = context.Game.Id,
                        Message = $"{context.CurrentPlayer.PlayerName} paid {propertyOwner.PlayerName} ${paymentAmount}"
                    });
                    await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                    {
                        Game = true,
                        Players = true,
                        GameLogs = true
                    });
                    return;
                }
            }
        }
        // Nothing needs to happen, front end can handle the next step
        await boardMovementService.ToggleOffGameMovement(context.Game.Id);
        await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
        {
            Game = true,
            GameLogs = true
        });
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
            if (utilityOwnerId == context.CurrentPlayer.Id)
            {
                await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                {
                    Game = true,
                    GameLogs = true
                });
                return;
            }
            //if someone else owns the railroad, pay them
            else
            {
                //if the property is mortgaged, don't pay so nothing happens
                if (landedOnUtility.Mortgaged is bool mortgaged && mortgaged == true)
                {
                    await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                    await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                    {
                        Game = true,
                        GameLogs = true
                    });
                    return;
                }
                //set up the player to roll for payment
                else
                {
                    await playerRepository.UpdateAsync(context.CurrentPlayer.Id, new PlayerUpdateParams { RollingForUtilities = true });
                    await gameLogRepository.CreateAsync(new GameLogCreateParams
                    {
                        GameId = context.Game.Id,
                        Message = $"{context.CurrentPlayer.PlayerName} landed on {context.LandedOnSpace.BoardSpaceName}, roll for payment amount."
                    });
                    await boardMovementService.ToggleOffGameMovement(context.Game.Id);
                    await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
                    {
                        Game = true,
                        Players = true,
                        GameLogs = true
                    });
                    return;
                }
            }
        }
        // Nothing needs to happen, front end can handle the next step
        await boardMovementService.ToggleOffGameMovement(context.Game.Id);
        await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
        {
            Game = true,
            GameLogs = true
        });
        return;
    }

    public async Task HandleLandedOnGoToJail(SpaceLandingServiceContext context)
    {
        //handle the initial land on the space
        await gameLogRepository.CreateAsync(new GameLogCreateParams
        {
            GameId = context.Game.Id,
            Message = $"{context.CurrentPlayer.PlayerName} was sent to jail."
        });
        await boardMovementService.ToggleOffGameMovement(context.Game.Id);
        await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
        {
            Game = true,
        });
        //send the player to jail
        await boardMovementService.ToggleOnGameMovement(context.Game.Id);
        await jailService.SendPlayerToJail(context.CurrentPlayer);
        await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
        {
            Game = true,
            Players = true
        });
        await boardMovementService.ToggleOffGameMovement(context.Game.Id);
        await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
        {
            Game = true,
            GameLogs = true
        });
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

        await paymentService.PayBank(context.CurrentPlayer, paymentAmount);
        
        await boardMovementService.ToggleOffGameMovement(context.Game.Id);
        await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
        {
            Players = true,
            GameLogs = true,
            Game = true
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
        await gameLogRepository.CreateAsync(new GameLogCreateParams
        {
            GameId = context.Game.Id,
            Message = card.CardDescription
        });
        await boardMovementService.ToggleOffGameMovement(context.Game.Id);

        //if the card had movement involved, need to handle landed on space again
        if (
            card.CardActionId == (int)CardActionIds.AdvanceToSpace ||
            card.CardActionId == (int)CardActionIds.BackThreeSpaces ||
            card.CardActionId == (int)CardActionIds.AdvanceToRailroad ||
            card.CardActionId == (int)CardActionIds.AdvanceToUtility ||
            card.CardActionId == (int)CardActionIds.GoToJail
        )
        {
            await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
            {
                Game = true,
                Players = true,
                GameLogs = true
            });
            //get the cards advance to space id, unless it's:
            // move back 3 spaces 
            // advance to utility
            // advance to nearest railroad
            BoardSpace movedToSpace;
            if (card.CardActionId == (int)CardActionIds.BackThreeSpaces)
            {
                int playerCurrentSpace = context.CurrentPlayer.BoardSpaceId;
                movedToSpace = context.BoardSpaces.First(bs => bs.Id == playerCurrentSpace - 3);
            }
            else if (card.CardActionId == (int)CardActionIds.AdvanceToUtility)
            {
                // if on or after waterworks, go to electric company
                if (context.CurrentPlayer.BoardSpaceId >= 29)
                {
                    movedToSpace = context.BoardSpaces.First(bs => bs.Id == 13);
                }
                // should go to water works if between electric company and water works
                else if (context.CurrentPlayer.BoardSpaceId >= 13 && context.CurrentPlayer.BoardSpaceId < 23)
                {
                    movedToSpace = context.BoardSpaces.First(bs => bs.Id == 29);
                }
                // go to electric company when current space < 13
                else
                {
                    movedToSpace = context.BoardSpaces.First(bs => bs.Id == 13);
                }
            }
            else if (card.CardActionId == (int)CardActionIds.AdvanceToRailroad)
            {
                int playerCurrentSpace = context.CurrentPlayer.BoardSpaceId;
                if (playerCurrentSpace >= 36)
                {
                    movedToSpace = context.BoardSpaces.First(bs => bs.Id == 6);
                }
                else
                {
                    var availableRailroads = context.BoardSpaces.Where(bs => bs.BoardSpaceCategoryId == (int)BoardSpaceCategories.Railroard);
                    BoardSpace nearestRailroad = availableRailroads.First(rr => rr.Id > playerCurrentSpace);
                    movedToSpace = nearestRailroad;
                }
            }
            else
            {
                movedToSpace = context.BoardSpaces.First(bs => bs.Id == card.AdvanceToSpaceId);
            }

            string message = movedToSpace.Id == 1
                ? $"{context.CurrentPlayer.PlayerName} advanced to {movedToSpace.BoardSpaceName} and collected $200."
                : $"{context.CurrentPlayer.PlayerName} advanced to {movedToSpace.BoardSpaceName}."
            ;
            await gameLogRepository.CreateAsync(new GameLogCreateParams
            {
                GameId = context.Game.Id,
                Message = message
            });
            await boardMovementService.ToggleOnGameMovement(context.Game.Id);
            await cardService.HandlePulledCard(card, context);
            await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
            {
                Game = true,
                Players = true
            });
            IEnumerable<Player> players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = context.Game.Id });
            await HandleLandedOnSpace(players, context.Game, true);
        }
        //no movement was involved, just send the details of the landed on space
        else
        {
            await cardService.HandlePulledCard(card, context);
            await socketMessageService.SendGameStateUpdate(context.Game.Id, new GameStateIncludeParams
            {
                Game = true,
                Players = true,
                GameLogs = true
            });
        }
    }   
}