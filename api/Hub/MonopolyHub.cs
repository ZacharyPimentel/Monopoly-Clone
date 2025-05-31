namespace api.hub
{
    using System.Data;
    using api.DTO.Entity;
    using api.DTO.Websocket;
    using api.Entity;
    using api.Enumerable;
    using api.Helper;
    using api.Interface;
    using Dapper;
    using Microsoft.AspNetCore.SignalR;
    public class MonopolyHub(
        GameState<MonopolyHub> gameState,
        IDbConnection db,
        IBoardSpaceRepository boardSpaceRepository,
        IGameCardRepository gameCardRepository,
        IGameLogRepository gameLogRepository,
        IGamePropertyRepository gamePropertyRepository,
        IGameRepository gameRepository,
        ILastDiceRollRepository lastDiceRollRepository,
        IPlayerRepository playerRepository,
        ITradeRepository tradeRepository,
        ITurnOrderRepository turnOrderRepository
    ) : Hub
    {
        //=======================================================
        // Default socket methods for connect / disconnect
        //=======================================================
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
        private async Task SendToAll(WebSocketEvents eventEnum, object data)
        {
            await Clients.All.SendAsync(((int)eventEnum).ToString(), data);
        }

        //=======================================================
        // Helper Methods for validating socket events
        //=======================================================
        private void ValidateGameId(Guid gameId)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if (gameId != currentSocketPlayer.GameId)
            {
                //TODO : add an error event in the socket
                throw new Exception("You aren't currently in this game");
            }
        }

        //=======================================================
        // Player 
        //=======================================================
        public async Task PlayerReconnect(Guid playerId)
        {
            var socketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams { Active = true });
            socketPlayer.PlayerId = playerId;
            var allPlayers = await playerRepository.GetAllWithIconsAsync();

            var currentPlayer = allPlayers.First(x => x.Id == playerId);
            await gameLogRepository.CreateAsync(new GameLogCreateParams
            {
                GameId = currentPlayer.GameId,
                Message = $"{currentPlayer.PlayerName} has reconnected."
            });
            var latestLogs = await gameLogRepository.GetLatestFive(currentPlayer.GameId);
            await SendToGroup(WebSocketEvents.GameLogUpdate, latestLogs);
            await SendToSelf(WebSocketEvents.PlayerUpdate, socketPlayer);
            await SendToGroup(WebSocketEvents.PlayerUpdateGroup, allPlayers);

            //trigger updated player counts in lobby
            var games = await gameRepository.Search(new GameWhereParams { });
            await SendToAll(WebSocketEvents.GameUpdateAll, games);
        }
        public async Task PlayerCreate(SocketEventPlayerCreate playerCreateParams)
        {
            ValidateGameId(playerCreateParams.GameId);
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            var newPlayer = await playerRepository.CreateAndReturnAsync(playerCreateParams);
            currentSocketPlayer.PlayerId = newPlayer.Id;
            var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams
            {
                GameId = currentSocketPlayer.GameId
            });
            await gameLogRepository.CreateAsync(new GameLogCreateParams
            {
                GameId = newPlayer.GameId,
                Message = $"{newPlayer.PlayerName} has joined the game."
            });
            var latestLogs = await gameLogRepository.GetLatestFive(newPlayer.GameId);
            await SendToGroup(WebSocketEvents.GameLogUpdate, latestLogs);
            await SendToSelf(WebSocketEvents.PlayerUpdate, currentSocketPlayer);
            await SendToGroup(WebSocketEvents.PlayerUpdateGroup, groupPlayers);

            //trigger updated player counts in lobby
            var games = await gameRepository.Search(new GameWhereParams { });
            await SendToAll(WebSocketEvents.GameUpdateAll, games);
        }
        public async Task PlayerUpdate(SocketEventPlayerUpdate playerUpdateParams)
        {
            await playerRepository.UpdateAsync(playerUpdateParams.PlayerId, playerUpdateParams.PlayerUpdateParams);
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if (currentSocketPlayer.GameId is not Guid gameId)
            {
                throw new Exception("Player does not have a GameId when it should be there.");
            }

            Game currentGame = await gameRepository.GetByIdAsync(gameId);
            var activeGroupPlayers = await playerRepository.SearchAsync(new PlayerWhereParams
            {
                Active = true,
                GameId = currentGame.Id
            },
            new {}
            );

            //if at least two players are all ready and the game is in lobby, start the game
            if (currentGame.InLobby && activeGroupPlayers.All(x => x.IsReadyToPlay == true) && activeGroupPlayers.AsList().Count >= 2)
            {
                await gameRepository.UpdateAsync(currentGame.Id, new GameUpdateParams
                {
                    InLobby = false,
                    GameStarted = true
                });
                await playerRepository.UpdateManyAsync(
                    new PlayerUpdateParams
                    {
                        InCurrentGame = true,
                        IsReadyToPlay = false,
                        Money = currentGame.StartingMoney
                    },
                    new PlayerWhereParams { Active = true },
                    new {}
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
                await SendToGroup(WebSocketEvents.GameUpdate, updatedGame);
            }
            var updatedGroupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams
            {
                GameId = currentSocketPlayer.GameId
            });

            await SendToGroup(WebSocketEvents.PlayerUpdateGroup, updatedGroupPlayers);
        }

        //=======================================================
        // Game
        //=======================================================
        public async Task GameGetAll()
        {
            var games = await gameRepository.GetAllAsync();
            await SendToSelf(WebSocketEvents.GameUpdateAll, games);
        }
        public async Task GameGetById(Guid gameId)
        {
            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToSelf(WebSocketEvents.GameUpdate, game);
        }
        public async Task GameCreate(GameCreateParams gameCreateParams)
        {
            var newGame = await gameRepository.CreateAndReturnAsync(gameCreateParams);
            //populate tables for new game
            await lastDiceRollRepository.CreateAsync(new { GameId = newGame.Id });
            await gamePropertyRepository.CreateForNewGameAsync(newGame.Id);
            await gameCardRepository.CreateForNewGameAsync(newGame.Id);
            await SendToSelf(WebSocketEvents.GameCreate, newGame.Id);;
            var games = await gameRepository.GetAllAsync();
            await SendToAll(WebSocketEvents.GameUpdateAll, games);
        }
        public async Task GameJoin(Guid gameId)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            currentSocketPlayer.GameId = gameId;
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            if (game == null)
            {
                await SendToSelf(WebSocketEvents.GameUpdate, game);
                return;
            }
            var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = gameId });
            var latestLogs = await gameLogRepository.GetLatestFive(game.Id);
            var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
            Console.WriteLine(trades);
            await SendToSelf(WebSocketEvents.GameUpdate, game);
            await SendToSelf(WebSocketEvents.PlayerUpdate, currentSocketPlayer);
            await SendToSelf(WebSocketEvents.GameUpdateGroup, groupPlayers);
            await SendToSelf(WebSocketEvents.GameLogUpdate, latestLogs);
            await SendToSelf(WebSocketEvents.TradeUpdate, trades);
            await BoardSpaceGetAll(gameId);
        }
        public async Task GameLeave(string gameId)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if (currentSocketPlayer.PlayerId is Guid playerId && currentSocketPlayer.GameId != null)
            {
                await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams { Active = false });
                var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = currentSocketPlayer.GameId });
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
                await SendToGroup(WebSocketEvents.PlayerUpdateGroup, groupPlayers);
                currentSocketPlayer.GameId = null;
                currentSocketPlayer.PlayerId = null;
                var games = await gameRepository.Search(new GameWhereParams { });
                await SendToAll(WebSocketEvents.GameUpdateAll, games);
            }
        }
        public async Task GameUpdate(Guid gameId, GameUpdateParams gameUpdateParams)
        {
            await gameRepository.UpdateAsync(gameId, gameUpdateParams);
            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToGroup(WebSocketEvents.GameUpdate, game);
        }
        public async Task GameEndTurn()
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);

            if (currentSocketPlayer.PlayerId is not Guid playerId || currentSocketPlayer.GameId == null)
            {
                throw new Exception("Socket player is missing data, PlayerId or GameId");
            }

            if (currentSocketPlayer.GameId is not Guid gameId)
            {
                throw new Exception("Game Id does not exist");
            }
            Game? currentGame = await gameRepository.GetByIdWithDetailsAsync(gameId) ?? throw new Exception("Game Doesn't Exist.");

            //keep the visuals of the dice in sync between real rolls and utility rolls
            if (currentGame.UtilityDiceOne != null && currentGame.UtilityDiceTwo != null)
            {
                await lastDiceRollRepository.UpdateManyAsync(
                    new LastDiceRollUpdateParams
                    {
                        DiceOne = currentGame.UtilityDiceOne,
                        DiceTwo = currentGame.UtilityDiceTwo,
                    },
                    new LastDiceRollWhereParams { GameId = currentGame.Id },
                    new { }
                );
            }
            
            await turnOrderRepository.UpdateManyAsync(
                new TurnOrderUpdateParams { HasPlayed = true},
                new TurnOrderWhereParams
                {
                    PlayerId = currentSocketPlayer.PlayerId,
                    GameId = currentGame.Id,
                },
                new {}
            );

            //check if everyone has taken their turn, reset if so
            var notPlayedCount = await db.ExecuteScalarAsync<int>(
                $"SELECT COUNT(*) FROM TurnOrder WHERE HasPlayed = FALSE AND GameId = @GameId",
                new {GameId = currentGame.Id}
            );


            if (notPlayedCount == 0)
            {
                await turnOrderRepository.UpdateManyAsync(
                    new TurnOrderUpdateParams { HasPlayed = false },
                    new TurnOrderWhereParams { GameId = currentGame.Id },
                    new { }
                );
                await playerRepository.UpdateManyAsync(
                    new PlayerUpdateParams { RollCount = 0 },
                    new PlayerWhereParams { InCurrentGame = true },
                    new { }
                );
            }

            await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams { TurnComplete = false });

            var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = gameId });
            var updatedGame = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToGroup(WebSocketEvents.PlayerUpdateGroup, groupPlayers);
            await SendToGroup(WebSocketEvents.GameUpdate, updatedGame);
        }
        public async Task BoardSpaceGetAll(Guid gameId)
        {
            var boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameId);

            await SendToSelf(WebSocketEvents.BoardSpaceUpdate, boardSpaces);
        }

        //=======================================================
        // Property
        //=======================================================
        public async Task GamePropertyUpdate(int gamePropertyId, GamePropertyUpdateParams updateParams)
        {
            await gamePropertyRepository.UpdateAsync(gamePropertyId, updateParams);
            GameProperty gameProperty = await gamePropertyRepository.GetByIdAsync(gamePropertyId);
            var boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameProperty.GameId);
            await SendToGroup(WebSocketEvents.BoardSpaceUpdate, boardSpaces);
        }

        //=======================================================
        // GameLog
        //=======================================================
        public async Task GameLogCreate(Guid gameId, string message)
        {
            await gameLogRepository.CreateAsync(new GameLogCreateParams
            {
                GameId = gameId,
                Message = message
            });
            var latestLogs = await gameLogRepository.GetLatestFive(gameId);
            await SendToGroup(WebSocketEvents.GameLogUpdate, latestLogs);
        }

        //=======================================================
        // Last Dice Roll
        //=======================================================
        public async Task LastDiceRollUpdate(Guid gameId, int diceOne, int diceTwo)
        {

            await lastDiceRollRepository.UpdateManyAsync(
                new LastDiceRollUpdateParams { DiceOne = diceOne, DiceTwo = diceTwo },
                new LastDiceRollWhereParams { GameId = gameId },
                new { }
            );

            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToGroup(WebSocketEvents.GameUpdate, game);
        }
        public async Task LastUtilityDiceRollUpdate(Guid gameId, int? diceOne, int? diceTwo)
        {

            await lastDiceRollRepository.UpdateManyAsync(
                new LastDiceRollUpdateParams { UtilityDiceOne = diceOne, UtilityDiceTwo = diceTwo },
                new LastDiceRollWhereParams { GameId = gameId },
                new { }
            );

            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToGroup(WebSocketEvents.GameUpdate, game);
        }
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

