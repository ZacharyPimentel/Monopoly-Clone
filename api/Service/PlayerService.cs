using System.Diagnostics;
using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.hub;
using api.Interface;
using api.Service.GameLogic;
using api.Socket;
using Dapper;
using Microsoft.AspNetCore.SignalR;

namespace api.Service;

public interface IPlayerService
{
    public Task 
    CreatePlayer(PlayerCreateParams playerCreateParams);
    public Task ReconnectToGame(Guid playerId);
    public Task EditPlayer(SocketEventPlayerEdit playerRenameParams);
    public Task RollForTurn(Player player, Game game);
    public Task SetPlayerReadyStatus(SocketEventPlayerReady playerReadyParams);
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
    IDiceRollService diceRollService
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
    public async Task EditPlayer(SocketEventPlayerEdit playerRenameParams)
    {
        if (playerRenameParams.PlayerId != CurrentSocketPlayer.PlayerId)
        {
            throw new Exception("You are not the player you're trying to rename");
        }
        await playerRepository.UpdateAsync(playerRenameParams.PlayerId, new PlayerUpdateParams
        {
            PlayerName = playerRenameParams.PlayerName,
            IconId = playerRenameParams.IconId
        });
        var allPlayers = await playerRepository.GetAllWithIconsAsync();
        await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, allPlayers);
    }
    public async Task SetPlayerReadyStatus(SocketEventPlayerReady playerReadyParams)
    {
        if (playerReadyParams.PlayerId != CurrentSocketPlayer.PlayerId)
        {
            throw new Exception("You are not the player you're trying to update");
        }
        await playerRepository.UpdateAsync(playerReadyParams.PlayerId, new PlayerUpdateParams
        {
            IsReadyToPlay = playerReadyParams.IsReadyToPlay
        });

        if (CurrentSocketPlayer.GameId is not Guid gameId)
        {
            throw new Exception("Player does not have a GameId when it should be there.");
        }
        Game currentGame = await gameRepository.GetByIdAsync((Guid)CurrentSocketPlayer.GameId);
        var activeGroupPlayers = await playerRepository.SearchAsync(new PlayerWhereParams
        {
            Active = true,
            GameId = currentGame.Id
        },
        new { }
        );

        //if at least two players are all ready and the game is in lobby, start the game
        if (currentGame.InLobby && activeGroupPlayers.All(x => x.IsReadyToPlay == true) && activeGroupPlayers.AsList().Count >= 2)
        {
            await gameRepository.UpdateAsync(currentGame.Id, new GameUpdateParams
            {
                InLobby = false,
                GameStarted = true
            });
            await playerRepository.UpdateWhereAsync(
                new PlayerUpdateParams
                {
                    InCurrentGame = true,
                    IsReadyToPlay = false,
                    Money = currentGame.StartingMoney
                },
                new PlayerWhereParams
                {
                    Active = true,
                    GameId = currentGame.Id
                },
                new { }
            );

            //randomize and set the turn order
            Random random = new();
            var shuffledActivePlayers = activeGroupPlayers.OrderBy(x => random.Next()).ToArray();
            foreach (var (player, index) in shuffledActivePlayers.Select((value, i) => (value, i)))
            {
                await turnOrderRepository.CreateAsync(new TurnOrderCreateParams
                {
                    PlayerId = player.Id,
                    GameId = currentGame.Id,
                    PlayOrder = index + 1
                });
            }

            Game? updatedGame = await gameRepository.GetByIdWithDetailsAsync(currentGame.Id);
            await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, updatedGame);
        }
        var updatedGroupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams
        {
            GameId = CurrentSocketPlayer.GameId
        });
        await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, updatedGroupPlayers);
    }
    public async Task RollForTurn(Player player,Game game)
    {
        var stopWatch = Stopwatch.StartNew();
        //send to the group that rolling is in progress before final save at the end
        game.DiceRollInProgress = true;
        await gameRepository.UpdateAsync(game.Id, new GameUpdateParams { DiceRollInProgress = true });
        await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, game);

        (int dieOne, int dieTwo) = await diceRollService.RollTwoDice();
        await diceRollService.RecordGameDiceRoll(game.Id, dieOne, dieTwo);

        //Handle jail logic
        if (player.InJail) {
            jailService.RunJailTurnLogic(player, dieOne, dieTwo);
        } else {
        //Handle player movement logic
            boardMovementService.MovePlayerWithDiceRoll(player, dieOne, dieTwo);
        }

        await playerRepository.UpdateAsync(player.Id, PlayerUpdateParams.FromPlayer(player));
        var gamePlayers = await playerRepository.SearchWithIconsAsync(
            new PlayerWhereParams { GameId = game.Id },
            new PlayerWhereParams { }
        );
        //update rolling to be finished
        await gameRepository.UpdateAsync(game!.Id, new GameUpdateParams { DiceRollInProgress = false });

        //wait one second while dice roll animation finishes
        stopWatch.Stop();

        //wait for at least 500ms for animations to finish.
        await Task.Delay(Math.Max(0, 500 - (int)stopWatch.ElapsedMilliseconds));

        Game? updatedGame = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        await socketMessageService.SendToGroup(WebSocketEvents.GameUpdate, updatedGame);
        await socketMessageService.SendToGroup(WebSocketEvents.PlayerUpdateGroup, gamePlayers);
    }
}