using System.Diagnostics;
using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.hub;
using api.Interface;
using api.Service.GameLogic;
using api.Socket;
using Dapper;
using Microsoft.AspNetCore.SignalR;

namespace api.Service;

public interface IPlayerService
{
    public Task CreatePlayer(PlayerCreateParams playerCreateParams);
    public Task ReconnectToGame(Guid playerId);
    public Task EditPlayer(Guid playerId, PlayerUpdateParams playerRenameParams);
    public Task RollForTurn(Player player, Game game);
    public Task RollForUtilities(Player player, Game game);

    public Task SetPlayerReadyStatus(Player player, Game game, bool isReadyToPlay);
    public Task PurchaseProperty(Player player, Game game, int gamePropertyId);
}

public class PlayerService(
    ISocketMessageService socketMessageService,
    GameState<MonopolyHub> gameState,
    ISocketContextAccessor socketContextAccessor,
    IPlayerRepository playerRepository,
    IGameLogRepository gameLogRepository,
    IGameRepository gameRepository,
    ITurnOrderRepository turnOrderRepository,
    ILastDiceRollRepository lastDiceRollRepository,
    IBoardSpaceRepository boardSpaceRepository,
    IJailService jailService,
    IBoardMovementService boardMovementService,
    IDiceRollService diceRollService,
    IGamePropertyRepository gamePropertyRepository,
    ISpaceLandingService spaceLandingService,
    IGameCardRepository gameCardRepository
) : IPlayerService
{
    private HubCallerContext SocketContext => socketContextAccessor.RequireContext().Context;
    private IHubContext<MonopolyHub> HubContext => socketContextAccessor.RequireContext().HubContext;

    private SocketPlayer CurrentSocketPlayer => gameState.GetPlayer(SocketContext.ConnectionId);

    public async Task CreatePlayer(PlayerCreateParams playerCreateParams)
    {
        var newPlayer = await playerRepository.CreateAndReturnAsync(playerCreateParams);
        CurrentSocketPlayer.PlayerId = newPlayer.Id;
        var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams
        {
            GameId = CurrentSocketPlayer.GameId
        });
        await gameLogRepository.CreateAsync(new GameLogCreateParams
        {
            GameId = newPlayer.GameId,
            Message = $"{newPlayer.PlayerName} has joined the game."
        });
        var latestLogs = await gameLogRepository.GetLatestFive(newPlayer.GameId);
        await socketMessageService.SendToGroup(WebSocketEvents.GameLogUpdate, latestLogs);
        await socketMessageService.SendToSelf(WebSocketEvents.PlayerUpdate, CurrentSocketPlayer);
        await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, groupPlayers);

        //trigger updated player counts in lobby
        var games = await gameRepository.Search(new GameWhereParams { });
        await socketMessageService.SendToAll(WebSocketEvents.GameUpdateAll, games);
    }
    public async Task ReconnectToGame(Guid playerId)
    {
        SocketPlayer currentSocketPlayer = gameState.GetPlayer(SocketContext.ConnectionId);
        await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams { Active = true });
        currentSocketPlayer.PlayerId = playerId;
        var allPlayers = await playerRepository.GetAllWithIconsAsync();

        var currentPlayer = allPlayers.First(x => x.Id == playerId);
        await gameLogRepository.CreateAsync(new GameLogCreateParams
        {
            GameId = currentPlayer.GameId,
            Message = $"{currentPlayer.PlayerName} has reconnected."
        });
        var latestLogs = await gameLogRepository.GetLatestFive(currentPlayer.GameId);
        await socketMessageService.SendToGroup(WebSocketEvents.GameLogUpdate, latestLogs);
        await socketMessageService.SendToSelf(WebSocketEvents.PlayerUpdate, currentSocketPlayer);
        await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, allPlayers);

        //trigger updated player counts in lobby
        var games = await gameRepository.Search(new GameWhereParams { });
        await socketMessageService.SendToAll(WebSocketEvents.GameUpdateAll, games);
    }
    public async Task EditPlayer(Guid playerId, PlayerUpdateParams playerRenameParams)
    {
        await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams
        {
            PlayerName = playerRenameParams.PlayerName,
            IconId = playerRenameParams.IconId
        });
        var allPlayers = await playerRepository.GetAllWithIconsAsync();
        await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, allPlayers);
    }
    public async Task SetPlayerReadyStatus(Player player, Game game, bool isReadyToPlay)
    {
        await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams
        {
            IsReadyToPlay = isReadyToPlay
        });

        var activeGroupPlayers = await playerRepository.SearchAsync(new PlayerWhereParams
        {
            Active = true,
            GameId = game.Id
        },
        new { }
        );

        //if at least two players are all ready and the game is in lobby, start the game
        if (game.InLobby && activeGroupPlayers.All(x => x.IsReadyToPlay == true) && activeGroupPlayers.AsList().Count >= 2)
        {
            await gameRepository.UpdateAsync(game.Id, new GameUpdateParams
            {
                InLobby = false,
                GameStarted = true
            });
            await playerRepository.UpdateWhereAsync(
                new PlayerUpdateParams
                {
                    InCurrentGame = true,
                    IsReadyToPlay = false,
                    CanRoll = false,
                    Money = game.StartingMoney
                },
                new PlayerWhereParams
                {
                    Active = true,
                    GameId = game.Id
                },
                new { }
            );

            //randomize and set the turn order
            Random random = new();
            var shuffledActivePlayers = activeGroupPlayers.OrderBy(x => random.Next()).ToArray();
            foreach (var (shuffledPlayer, index) in shuffledActivePlayers.Select((value, i) => (value, i)))
            {
                TurnOrderCreateParams turnOrderCreateParams = new()
                {
                    PlayerId = shuffledPlayer.Id,
                    GameId = game.Id,
                    PlayOrder = index + 1
                };

                //if the first player, set them to be allowed to roll
                if (index == 0)
                {
                    await playerRepository.UpdateAsync(shuffledPlayer.Id, new PlayerUpdateParams { CanRoll = true });
                }
                
                await turnOrderRepository.CreateAsync(turnOrderCreateParams);
            }

            Game? updatedGame = await gameRepository.GetByIdWithDetailsAsync(game.Id);
            await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, updatedGame);
        }
        var updatedGroupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams
        {
            GameId = game.Id
        });
        await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, updatedGroupPlayers);
    }
    public async Task RollForTurn(Player player, Game game)
    {
        var stopWatch = Stopwatch.StartNew();
        //send to the group that rolling is in progress before final save at the end
        game.DiceRollInProgress = true;
        await gameRepository.UpdateAsync(game.Id, new GameUpdateParams { DiceRollInProgress = true });
        await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, game);

        (int dieOne, int dieTwo) = await diceRollService.RollTwoDice();
        await diceRollService.RecordGameDiceRoll(game.Id, dieOne, dieTwo);

        //Handle jail logic
        string jailMessage = string.Empty;
        string rollMessage = string.Empty;
        if (player.InJail)
        {
            jailMessage = jailService.RunJailTurnLogic(player, game, dieOne, dieTwo);
        }
        else
        {
            //Handle player movement logic
            rollMessage = boardMovementService.MovePlayerWithDiceRoll(player, game, dieOne, dieTwo);
        }

        await playerRepository.UpdateAsync(player.Id, PlayerUpdateParams.FromPlayer(player));
        var gamePlayers = await playerRepository.SearchWithIconsAsync(
            new PlayerWhereParams { GameId = game.Id },
            new PlayerWhereParams { }
        );
        //update rolling to be finished
        await gameRepository.UpdateAsync(game!.Id, new GameUpdateParams { DiceRollInProgress = false, MovementInProgress = true });
        Game updatedGame = await gameRepository.GetByIdWithDetailsAsync(game.Id);

        //wait one second while dice roll animation finishes
        stopWatch.Stop();

        //wait for at least 500ms for animations to finish.
        await Task.Delay(Math.Max(0, 500 - (int)stopWatch.ElapsedMilliseconds));

        await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, updatedGame);
        await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, gamePlayers);
        if (jailMessage != string.Empty)
        {
            await socketMessageService.CreateAndSendLatestGameLogs(game.Id, jailMessage);
        }
        if (rollMessage != string.Empty)
        {
            await socketMessageService.CreateAndSendLatestGameLogs(game.Id, rollMessage);
        }

        //run all the logic needed to handle the space that was landed on
        //includes sending out socket events
        await spaceLandingService.HandleLandedOnSpace(gamePlayers, updatedGame);
    }

    public async Task RollForUtilities(Player player, Game game)
    {
        if (!player.RollingForUtilities)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerNotRollingForUtilities);
            throw new Exception(errorMessage);
        }

        var stopWatch = Stopwatch.StartNew();
        //send to the group that rolling is in progress before final save at the end
        game.DiceRollInProgress = true;
        await gameRepository.UpdateAsync(game.Id, new GameUpdateParams { DiceRollInProgress = true });
        await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, game);
        (int dieOne, int dieTwo) = await diceRollService.RollTwoDice();
        await diceRollService.RecordGameUtilityDiceRoll(game.Id, dieOne, dieTwo);
        IEnumerable<BoardSpace> boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(game.Id);
        BoardSpace currentSpace = boardSpaces.First(bs => bs.Id == player.BoardSpaceId);

        if (currentSpace.Property?.PlayerId is not Guid propertyOwnerId)
        {
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.PropertyNotOwned));
        }

        int ownerNumberOfUtilties = boardSpaces.Where(bs =>
            bs.BoardSpaceCategoryId == (int)BoardSpaceCategories.Utility &&
            bs.Property?.PlayerId == propertyOwnerId
        ).Count();


        GameCard? lastPlayedCard = await gameCardRepository.GetLastPlayedGameCard(game.Id);
        bool needsToPay10x= false;
        //if the last card was advance to utility, need to pay owner 10x regardless of how many utilities are owned.
        if (lastPlayedCard != null && lastPlayedCard?.Card?.CardActionId == (int)CardActionIds.AdvanceToUtility)
        {
            needsToPay10x = true;
        }

            int amountToPay = 0;
        if (ownerNumberOfUtilties == 1)
        {
            //4x multiplier
            amountToPay = (dieOne + dieTwo) * 4;
        }
        if (ownerNumberOfUtilties == 2 || needsToPay10x)
        {
            //10x multiplier
            amountToPay = (dieOne + dieTwo) * 10;
        }

        Player propertyOwner = await playerRepository.GetByIdAsync(propertyOwnerId);
        await playerRepository.UpdateAsync(propertyOwnerId, new PlayerUpdateParams
        {
            Money = propertyOwner.Money + amountToPay
        });
        await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams
        {
            Money = player.Money - amountToPay
        });
        await gameLogRepository.CreateAsync(new GameLogCreateParams
        {
            GameId = game.Id,
            Message = $"{player.PlayerName} paid {propertyOwner.PlayerName} ${amountToPay}."
        });
        //update rolling to be finished
        await gameRepository.UpdateAsync(game.Id, new GameUpdateParams { DiceRollInProgress = false});
        Game updatedGame = await gameRepository.GetByIdWithDetailsAsync(game.Id);

        //wait one second while dice roll animation finishes
        stopWatch.Stop();

        //wait for at least 500ms for animations to finish.
        await Task.Delay(Math.Max(0, 500 - (int)stopWatch.ElapsedMilliseconds));

        await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, updatedGame);
        await socketMessageService.SendGamePlayers(game.Id);
    }

    public async Task PurchaseProperty(Player player, Game game, int gamePropertyId)
    {
        GameProperty gameProperty = await gamePropertyRepository.GetByIdWithDetailsAsync(gamePropertyId);

        if (player.BoardSpaceId != gameProperty.BoardSpaceId)
        {
            string errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerBoardSpaceMismatch);
            throw new Exception(errorMessage);
        }
        if (gameProperty.PlayerId != null)
        {
            string errorMessage = EnumExtensions.GetEnumDescription(Errors.PropertyAlreadyOwned);
            throw new Exception(errorMessage);
        }

        if (player.Money - gameProperty.PurchasePrice < 0)
        {
            throw new Exception(EnumExtensions.GetEnumDescription(Errors.NotEnoughMoney));
        }

        await gamePropertyRepository.UpdateAsync(gamePropertyId, new GamePropertyUpdateParams { PlayerId = player.Id });
        await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams { Money = player.Money - gameProperty.PurchasePrice });
        await gameLogRepository.CreateAsync(new GameLogCreateParams
        {
            GameId = game.Id,
            Message = $"{player.PlayerName} purchased {gameProperty.BoardSpaceName}"
        });

        await socketMessageService.SendGamePlayers(game.Id);
        await socketMessageService.SendGameBoardSpaces(game.Id);
    }
}