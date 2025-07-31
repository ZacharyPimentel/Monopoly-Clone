namespace api.hub
{
    using api.DTO.Entity;
    using api.DTO.Websocket;
    using api.Enumerable;
    using api.Hubs;
    using api.Interface;
    using api.Service;
    using api.Service.GameLogic;
    using api.Service.GuardService;
    using Microsoft.AspNetCore.SignalR;
    public class MonopolyHub(
        GameState<MonopolyHub> gameState,
        IBoardSpaceRepository boardSpaceRepository,
        IGameLogRepository gameLogRepository,
        IGameRepository gameRepository,
        IPlayerRepository playerRepository,
        ITradeRepository tradeRepository,
        IGameService gameService,
        IPlayerService playerService,
        IGuardService guardService,
        IJailService jailService,
        ITradeService tradeService,
        IPropertyService propertyService,
        IPaymentService paymentService,
        ISpaceLandingService spaceLandingService
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
                await gameLogRepository.CreateAsync(new GameLogCreateParams
                {
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
                .Init(playerId, currentSocketPlayer.GameId);

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
                .Init(null, currentSocketPlayer.GameId);

            guards.GameExists();

            await playerService.CreatePlayer(new PlayerCreateParams
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
                    .IsCurrentTurn()
                    .PlayerAllowedToRoll();

                await playerService.RollForTurn(guardService.GetPlayer(), guardService.GetGame());
            });
        }
        public async Task PayOutOfJail()
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

        //=======================================================
        // Game
        //=======================================================
        public async Task GameGetAll()
        {
            var games = await gameRepository.GetAllWithPlayerCountAsync();
            var activeGames = games.Where(g => g.GameOver == false);
            await SendToSelf(WebSocketEvents.GameUpdateAll, activeGames);
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
                    .Init(null, gameId);

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
                    .Init(null, gameId);

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

                await gameService.UpdateRules(guardService.GetGame().Id, rulesUpdateParams);
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

                await gameService.EndTurn(guardService.GetPlayer(), guardService.GetGame());
            });

        }
        public async Task BoardSpaceGetAll(Guid gameId)
        {
            var boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameId);

            await SendToSelf(WebSocketEvents.BoardSpaceUpdate, boardSpaces);
        }

        //=======================================================
        // Trade
        //=======================================================
        public async Task TradeCreate(SocketEventTradeCreate tradeCreateParams)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasGameId()
                    .SocketConnectionHasPlayerId()
                    .InitMultiple(
                        [
                            tradeCreateParams.PlayerOne.PlayerId,
                            tradeCreateParams.PlayerTwo.PlayerId
                        ],
                        currentSocketPlayer.GameId
                    );
                guards
                    .PlayersExist()
                    .GameExists()
                    .PlayersAreInCorrectGame()
                    .PlayerIdInList([tradeCreateParams.PlayerOne.PlayerId, tradeCreateParams.PlayerTwo.PlayerId])
                    .PlayerIdsAreInList([tradeCreateParams.PlayerOne.PlayerId, tradeCreateParams.PlayerTwo.PlayerId]);

                await tradeService.CreateGameTrade(new TradeCreateParams
                {
                    Initiator = guardService.GetCurrentPlayerFromList().Id,
                    GameId = guardService.GetGame().Id,
                    PlayerOne = tradeCreateParams.PlayerOne,
                    PlayerTwo = tradeCreateParams.PlayerTwo
                });
            });
        }
        public async Task TradeSearch(Guid gameId)
        {
            var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
            await SendToSelf(WebSocketEvents.TradeList, trades);
        }
        public async Task TradeUpdate(SocketEventTradeUpdate tradeUpdateParams)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasGameId()
                    .SocketConnectionHasPlayerId()
                    .InitMultiple(
                        [
                            tradeUpdateParams.PlayerOne.PlayerId,
                            tradeUpdateParams.PlayerTwo.PlayerId
                        ],
                        currentSocketPlayer.GameId
                    );
                guards
                    .PlayersExist()
                    .GameExists()
                    .PlayersAreInCorrectGame()
                    .PlayerIdInList([tradeUpdateParams.PlayerOne.PlayerId, tradeUpdateParams.PlayerTwo.PlayerId])
                    .PlayerIdsAreInList([tradeUpdateParams.PlayerOne.PlayerId, tradeUpdateParams.PlayerTwo.PlayerId]);

                await tradeService.UpdateGameTrade(
                    tradeUpdateParams.TradeId,
                    guardService.GetGame().Id,
                    new TradeUpdateParams
                    {
                        PlayerOne = tradeUpdateParams.PlayerOne,
                        PlayerTwo = tradeUpdateParams.PlayerTwo,
                        LastUpdatedBy = guardService.GetPlayer().Id
                    });
            });
        }
        public async Task TradeDecline(SocketEventTradeDecline declineParams)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasGameId()
                    .SocketConnectionHasPlayerId()
                    .Init(currentSocketPlayer.PlayerId);
                guards
                    .PlayersExist();

                await tradeService.DeclineTrade(guardService.GetPlayer(), declineParams.TradeId);
            });

        }

        public async Task TradeAccept(SocketEventTradeAccept acceptParams)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasGameId()
                    .SocketConnectionHasPlayerId()
                    .Init(currentSocketPlayer.PlayerId);
                guards
                    .PlayersExist();

                await tradeService.AcceptTrade(guardService.GetPlayer(), acceptParams.TradeId);
            });

        }

        public async Task PropertyMortgage(int gamePropertyId)
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
                    .IsCurrentTurn();

                await propertyService.MortgageProperty(gamePropertyId);
            });
        }
        public async Task PropertyUnmortgage(int gamePropertyId)
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
                    .IsCurrentTurn();

                await propertyService.UnmortgageProperty(gamePropertyId);
            });
        }
        public async Task PropertyUpgrade(int gamePropertyId)
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
                    .IsCurrentTurn();

                await propertyService.UpgradeProperty(gamePropertyId);
            });
        }

        public async Task PropertyDowngrade(int gamePropertyId)
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
                    .IsCurrentTurn();

                await propertyService.DowngradeProperty(gamePropertyId);
            });
        }

        public async Task PlayerGoBankrupt()
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
                    .IsCurrentTurn();

                await playerService.DeclareBankruptcy();
            });
        }

        public async Task PlayerCompletePayment()
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await guardService.HandleGuardError(async () =>
            {
                IGuardClause guards = await guardService
                    .SocketConnectionHasPlayerId()
                    .SocketConnectionHasGameId()
                    .Init(currentSocketPlayer.PlayerId,currentSocketPlayer.GameId);
                guards
                    .PlayerExists()
                    .IsCurrentTurn();

                await paymentService.PayDebt(guardService.GetPlayer());
            });
        }
    }
}

