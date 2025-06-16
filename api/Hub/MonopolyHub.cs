namespace api.hub
{
    using System.Data;
    using api.DTO.Entity;
    using api.DTO.Websocket;
    using api.Entity;
    using api.Enumerable;
    using api.Interface;
    using api.Service;
    using api.Service.GameLogic;
    using api.Service.GuardService;
    using Dapper;
    using Microsoft.AspNetCore.SignalR;
    public class MonopolyHub(
        GameState<MonopolyHub> gameState,
        IBoardSpaceRepository boardSpaceRepository,
        IGameLogRepository gameLogRepository,
        //IGamePropertyRepository gamePropertyRepository,
        IGameRepository gameRepository,
        //ILastDiceRollRepository lastDiceRollRepository,
        IPlayerRepository playerRepository,
        ITradeRepository tradeRepository,
        //ITurnOrderRepository turnOrderRepository,
        IGameService gameService,
        IPlayerService playerService,
        IGuardService guardService,
        IJailService jailService
        //ISocketMessageService socketMessageService
    ) : Hub
    {
        //=======================================================
        // Default socket methods for connect / disconnect
        //===================================================`====
        public override Task OnConnectedAsync()
        {
            var newPlayer = new SocketPlayer { SocketId = Context.ConnectionId };
            gameState.Players.Add(newPlayer);
            return base.OnConnectedAsync();
        }
        public override async Task<Task> OnDisconnectedAsync(Exception? exception)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            //if the socket player has a game player id, update to inactive
            if (currentSocketPlayer.PlayerId is Guid playerId && currentSocketPlayer.GameId != null)
            {
                await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams { Active = false });
                var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = currentSocketPlayer.GameId });
                var gamePlayer = groupPlayers.First(x => x.Id == currentSocketPlayer.PlayerId);
                await gameLogRepository.CreateAsync(new GameLogCreateParams{
                    GameId = gamePlayer.GameId,
                    Message = $"{gamePlayer.PlayerName} has disconnected.",
                });
                var latestLogs = await gameLogRepository.GetLatestFive(gamePlayer.GameId);
                await SendToGroup(WebSocketEvents.PlayerUpdateGroup, groupPlayers);
                await SendToGroup(WebSocketEvents.GameLogUpdate, latestLogs);
            }
            gameState.RemovePlayer(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
        //=======================================================
        // Methods for message delivery (self, group, or all)
        //=======================================================
        private async Task SendToSelf(WebSocketEvents eventEnum, object? data)
        {
            await Clients.Caller.SendAsync(((int)eventEnum).ToString(), data);
        }
        private async Task SendToGroup(WebSocketEvents eventEnum, object? data)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if (currentSocketPlayer.GameId is Guid gameId)
            {
                await Clients.Group(gameId.ToString()).SendAsync(((int)eventEnum).ToString(), data);
            }
            else
            {
                throw new Exception("Tried to send data to a group where the GameId was not found.");
            }

            await Clients.Caller.SendAsync(((int)eventEnum).ToString(), data);
        }
        // private async Task SendToAll(WebSocketEvents eventEnum, object data)
        // {
        //     await Clients.All.SendAsync(((int)eventEnum).ToString(), data);
        // }

        //=======================================================
        // Player 
        //=======================================================
        public async Task PlayerReconnect(Guid playerId)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            IGuardClause guards = await guardService
                .SocketConnectionDoesNotHavePlayerId()
                .Init(playerId,currentSocketPlayer.GameId);

            guards
                .PlayerIsInactive()
                .PlayerIsInCorrectGame();          

            await playerService.ReconnectToGame(playerId);
        }
        public async Task PlayerCreate(SocketEventPlayerCreate playerCreateParams)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);

            IGuardClause guards = await guardService
                .SocketConnectionDoesNotHavePlayerId()
                .Init(null,currentSocketPlayer.GameId);

            guards.GameExists();

            await playerService.CreatePlayer( new PlayerCreateParams
            {
                PlayerName = playerCreateParams.PlayerName,
                IconId = playerCreateParams.IconId,
                GameId = guardService.GetGame().Id    
            });
        }
        public async Task PlayerEdit(SocketEventPlayerEdit editParams)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasPlayerId()
                    .Init(currentSocketPlayer.PlayerId);
                guards.PlayerExists();
                
                await playerService.EditPlayer(guardService.GetPlayer().Id, new PlayerUpdateParams
                {
                    PlayerName = editParams.PlayerName,
                    IconId = editParams.IconId
                });
            });
        }
        public async Task PlayerReady(SocketEventPlayerReady playerReadyParams)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasPlayerId()
                    .SocketConnectionHasGameId()
                    .Init(currentSocketPlayer.PlayerId, currentSocketPlayer.GameId);

                guards
                    .PlayerExists()
                    .GameExists();
            });
            await playerService.SetPlayerReadyStatus(
                guardService.GetPlayer(),
                guardService.GetGame(),
                playerReadyParams.IsReadyToPlay
            );
        }
        public async Task PlayerRollForTurn()
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);

            await guardService.HandleGuardError(async() =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasPlayerId()
                    .SocketConnectionHasGameId()
                    .Init(currentSocketPlayer.PlayerId,currentSocketPlayer.GameId);

                guards
                    .PlayerExists()
                    .GameExists()
                    .PlayerIsInCorrectGame()
                    .IsCurrentTurn()
                    .PlayerAllowedToRoll();

                await playerService.RollForTurn(guardService.GetPlayer(), guardService.GetGame());
            });
        }
        public async Task PayOutOfJail()
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
             await guardService.HandleGuardError(async() =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasPlayerId()
                    .SocketConnectionHasGameId()
                    .Init(currentSocketPlayer.PlayerId, currentSocketPlayer.GameId);

                guards
                    .PlayerExists()
                    .GameExists()
                    .PlayerIsInCorrectGame()
                    .IsCurrentTurn()
                    .PlayerAllowedToRoll()
                    .PlayerInJail();

                await jailService.PayOutOfJail(guardService.GetPlayer());
            });
        }
        public async Task PlayerRollForUtilties()
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);

            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasPlayerId()
                    .SocketConnectionHasGameId()
                    .Init(currentSocketPlayer.PlayerId, currentSocketPlayer.GameId);

                guards
                    .PlayerExists()
                    .GameExists()
                    .PlayerIsInCorrectGame()
                    .IsCurrentTurn();

                await playerService.RollForUtilities(guardService.GetPlayer(), guardService.GetGame());
            });
        }

        public async Task PlayerPurchaseProperty(SocketEventPurchaseProperty purchasePropertyParams)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasGameId()
                    .SocketConnectionHasPlayerId()
                    .Init(currentSocketPlayer.PlayerId, currentSocketPlayer.GameId);

                guards
                    .GameExists()
                    .PlayerExists()
                    .PlayerIsInCorrectGame()
                    .IsCurrentTurn();

                await playerService.PurchaseProperty(
                    guardService.GetPlayer(),
                    guardService.GetGame(),
                    purchasePropertyParams.GamePropertyId
                );
            });
        }
        // public async Task PlayerUpdate(SocketEventPlayerUpdate playerUpdateParams)
        // {
        //     SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
        //     if (currentSocketPlayer.GameId is not Guid gameId)
        //     {
        //         throw new Exception("Player does not have a GameId when it should be there.");
        //     }
        //     await playerRepository.UpdateAsync(playerUpdateParams.PlayerId, playerUpdateParams.PlayerUpdateParams);

        //     var updatedGroupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams
        //     {
        //         GameId = currentSocketPlayer.GameId
        //     });

        //     await SendToGroup(WebSocketEvents.PlayerUpdateGroup, updatedGroupPlayers);
        // }

        //=======================================================
        // Game
        //=======================================================
        public async Task GameGetAll()
        {
            var games = await gameRepository.GetAllWithPlayerCountAsync();
            await SendToSelf(WebSocketEvents.GameUpdateAll, games);
        }
        public async Task GameCreate(GameCreateParams gameCreateParams)
        {
            await guardService.HandleGuardError(async () =>
            {
                await gameService.CreateGame(gameCreateParams);
            });
        }
        public async Task GameJoin(Guid gameId)
        {
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionDoesNotHavePlayerId()
                    .SocketConnectionDoesNotHaveGameId()
                    .Init(null,gameId);

                guards.GameExists();
            
                await gameService.JoinGame(gameId);
            });
        }
        public async Task GameLeave(Guid gameId)
        {
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasGameId()
                    .Init(null,gameId);

                guards.GameExists();
            
                await gameService.LeaveGame(gameId);
            });
        }
        public async Task GameRulesUpdate(SocketEventRulesUpdate rulesUpdateParams)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasGameId()
                    .SocketConnectionHasPlayerId()
                    .Init(currentSocketPlayer.PlayerId, currentSocketPlayer.GameId);

                guards
                    .PlayerExists()
                    .GameExists()
                    .PlayerIsInCorrectGame()
                    .GameNotStarted();

                await gameService.UpdateRules(guardService.GetGame().Id,rulesUpdateParams);
            });
        }
        public async Task GameEndTurn()
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasGameId()
                    .SocketConnectionHasPlayerId()
                    .Init(currentSocketPlayer.PlayerId, currentSocketPlayer.GameId);

                guards
                    .PlayerExists()
                    .GameExists()
                    .IsCurrentTurn()
                    .PlayerNotAllowedToRoll();
                
                await gameService.EndTurn(guardService.GetPlayer(),guardService.GetGame());
            });
            
        }
        public async Task BoardSpaceGetAll(Guid gameId)
        {
            var boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameId);

            await SendToSelf(WebSocketEvents.BoardSpaceUpdate, boardSpaces);
        }

        //=======================================================
        // Property
        //=======================================================
        // public async Task GamePropertyUpdate(int gamePropertyId, GamePropertyUpdateParams updateParams)
        // {
        //     await gamePropertyRepository.UpdateAsync(gamePropertyId, updateParams);
        //     GameProperty gameProperty = await gamePropertyRepository.GetByIdAsync(gamePropertyId);
        //     var boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameProperty.GameId);
        //     await SendToGroup(WebSocketEvents.BoardSpaceUpdate, boardSpaces);
        // }

        //=======================================================
        // GameLog
        //=======================================================
        // public async Task GameLogCreate(Guid gameId, string message)
        // {
        //     await gameLogRepository.CreateAsync(new GameLogCreateParams
        //     {
        //         GameId = gameId,
        //         Message = message
        //     });
        //     var latestLogs = await gameLogRepository.GetLatestFive(gameId);
        //     await SendToGroup(WebSocketEvents.GameLogUpdate, latestLogs);
        // }

        //=======================================================
        // Last Dice Roll
        //=======================================================
        // public async Task LastDiceRollUpdate(Guid gameId, int diceOne, int diceTwo)
        // {

        //     await lastDiceRollRepository.UpdateWhereAsync(
        //         new LastDiceRollUpdateParams { DiceOne = diceOne, DiceTwo = diceTwo },
        //         new LastDiceRollWhereParams { GameId = gameId },
        //         new { }
        //     );

        //     Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
        //     await SendToGroup(WebSocketEvents.GameUpdate, game);
        // }
        // public async Task LastUtilityDiceRollUpdate(Guid gameId, int? diceOne, int? diceTwo)
        // {

        //     await lastDiceRollRepository.UpdateWhereAsync(
        //         new LastDiceRollUpdateParams { UtilityDiceOne = diceOne, UtilityDiceTwo = diceTwo },
        //         new LastDiceRollWhereParams { GameId = gameId },
        //         new { }
        //     );

        //     Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
        //     await SendToGroup(WebSocketEvents.GameUpdate, game);
        // }
        //=======================================================
        // Trade
        //=======================================================
        public async Task TradeCreate(TradeCreateParams tradeCreateParams)
        {
            await tradeRepository.CreateFullTradeAsync(tradeCreateParams);
            var trades = await tradeRepository.GetActiveFullTradesForGameAsync(tradeCreateParams.GameId);
            await SendToGroup(WebSocketEvents.TradeUpdate, trades);
        }
        public async Task TradeSearch(Guid gameId)
        {
            var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
            await SendToSelf(WebSocketEvents.TradeList, trades);
        }
        public async Task TradeUpdate(SocketEventTradeUpdate socketEventData)
        {

            var socketPlayer = gameState.GetPlayer(Context.ConnectionId);
            socketEventData.TradeUpdateParams.LastUpdatedBy = socketPlayer.PlayerId;
            await tradeRepository.UpdateFullTradeAsync(
                socketEventData.TradeId,
                socketEventData.TradeUpdateParams
            );
            if (socketPlayer.GameId is Guid gameId)
            {
                var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
                await SendToGroup(WebSocketEvents.TradeUpdate, trades);
            }
        }
        public async Task TradeDecline(int tradeId)
        {
            var socketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if (socketPlayer.GameId is Guid gameId)
            {
                await tradeRepository.DeclineTrade(tradeId, gameId);
                var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = socketPlayer.GameId });
                var gamePlayer = groupPlayers.First(x => x.Id == socketPlayer.PlayerId);
                await gameLogRepository.CreateAsync(new GameLogCreateParams
                {
                    GameId = gameId,
                    Message = $"{gamePlayer.PlayerName} has disconnected."
                });
                var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
                await SendToGroup(WebSocketEvents.TradeUpdate, trades);
            }
        }
    }
}

