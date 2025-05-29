namespace api.hub
{
    using System.Data;
    using api.Entity;
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
                await SendToGroup("player:updateGroup", groupPlayers);
                await SendToGroup("gameLog:update", latestLogs);
            }
            gameState.RemovePlayer(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
        //=======================================================
        // Methods for message delivery (self, group, or all)
        //=======================================================
        private async Task SendToSelf(string eventName, object? data)
        {
            await Clients.Caller.SendAsync(eventName, data);
        }
        private async Task SendToGroup(string eventName, object? data)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if (currentSocketPlayer.GameId is Guid gameId)
            {
                await Clients.Group(gameId.ToString()).SendAsync(eventName, data);
            }
            else
            {
                throw new Exception("Tried to send data to a group where the GameId was not found.");
            }

            await Clients.Caller.SendAsync(eventName, data);
        }
        private async Task SendToAll(string eventName, object data)
        {
            await Clients.All.SendAsync(eventName, data);
        }

        //=======================================================
        // Player 
        //=======================================================
        public async Task PlayerReconnect(Guid playerId)
        {
            var socketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await playerRepository.UpdateAsync(playerId, new PlayerUpdateParams { Active = true });
            socketPlayer.PlayerId = playerId;
            var allPlayers = await playerRepository.GetAllAsync();

            var currentPlayer = allPlayers.First(x => x.Id == playerId);
            await gameLogRepository.CreateAsync(new GameLogCreateParams
            {
                GameId = currentPlayer.GameId,
                Message = $"{currentPlayer.PlayerName} has reconnected."
            });
            var latestLogs = await gameLogRepository.GetLatestFive(currentPlayer.GameId);
            await SendToGroup("gameLog:update", latestLogs);
            await SendToSelf("player:update", socketPlayer);
            await SendToGroup("player:updateGroup", allPlayers);

            //trigger updated player counts in lobby
            var games = await gameRepository.Search(new GameWhereParams { });
            await SendToAll("game:updateAll", games);
        }
        public async Task PlayerCreate(PlayerCreateParams playerCreateParams)
        {
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
            await SendToGroup("gameLog:update", latestLogs);
            await SendToSelf("player:update", currentSocketPlayer);
            await SendToGroup("player:updateGroup", groupPlayers);

            //trigger updated player counts in lobby
            var games = await gameRepository.Search(new GameWhereParams { });
            await SendToAll("game:updateAll", games);
        }
        public async Task PlayerUpdate(Guid playerId, PlayerUpdateParams playerUpdateParams)
        {
            await playerRepository.UpdateAsync(playerId, playerUpdateParams);
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
                await SendToGroup("game:update", updatedGame);
            }
            var updatedGroupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams
            {
                GameId = currentSocketPlayer.GameId
            });

            await SendToGroup("player:updateGroup", updatedGroupPlayers);
        }

        //=======================================================
        // Game
        //=======================================================
        public async Task GameGetAll()
        {
            var games = await gameRepository.GetAllAsync();
            await SendToSelf("game:updateAll", games);
        }
        public async Task GameGetById(Guid gameId)
        {
            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToSelf("game:update", game);
        }
        public async Task GameCreate(GameCreateParams gameCreateParams)
        {
            var newGame = await gameRepository.CreateAndReturnAsync(gameCreateParams);
            //populate tables for new game
            await lastDiceRollRepository.CreateAsync(new { GameId = newGame.Id });
            await gamePropertyRepository.CreateForNewGameAsync(newGame.Id);
            await gameCardRepository.CreateForNewGameAsync(newGame.Id);
            await SendToSelf("game:create", newGame.Id);;
            var games = await gameRepository.GetAllAsync();
            await SendToAll("game:updateAll", games);
        }
        public async Task GameJoin(Guid gameId)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            currentSocketPlayer.GameId = gameId;
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId.ToString());
            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            if (game == null)
            {
                await SendToSelf("game:update", game);
                return;
            }
            var groupPlayers = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = gameId });
            var latestLogs = await gameLogRepository.GetLatestFive(game.Id);
            var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
            Console.WriteLine(trades);
            await SendToSelf("game:update", game);
            await SendToSelf("player:update", currentSocketPlayer);
            await SendToSelf("player:updateGroup", groupPlayers);
            await SendToSelf("gameLog:update", latestLogs);
            await SendToSelf("trade:update", trades);
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
                await SendToGroup("player:updateGroup", groupPlayers);
                currentSocketPlayer.GameId = null;
                currentSocketPlayer.PlayerId = null;
                var games = await gameRepository.Search(new GameWhereParams { });
                await SendToAll("game:updateAll", games);
            }
        }
        public async Task GameUpdate(Guid gameId, GameUpdateParams gameUpdateParams)
        {
            await gameRepository.UpdateAsync(gameId, gameUpdateParams);
            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToGroup("game:Update", game);
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
            await SendToGroup("player:updateGroup", groupPlayers);
            await SendToGroup("game:update", updatedGame);
        }
        public async Task BoardSpaceGetAll(Guid gameId)
        {
            var boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameId);

            await SendToSelf("boardSpace:update", boardSpaces);
        }

        //=======================================================
        // Property
        //=======================================================
        public async Task GamePropertyUpdate(int gamePropertyId, GamePropertyUpdateParams updateParams)
        {
            await gamePropertyRepository.UpdateAsync(gamePropertyId, updateParams);
            GameProperty gameProperty = await gamePropertyRepository.GetByIdAsync(gamePropertyId);
            var boardSpaces = await boardSpaceRepository.GetAllForGameWithDetailsAsync(gameProperty.GameId);
            await SendToGroup("boardSpace:update", boardSpaces);
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
            await SendToGroup("gameLog:update", latestLogs);
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
            await SendToGroup("game:update", game);
        }
        public async Task LastUtilityDiceRollUpdate(Guid gameId, int? diceOne, int? diceTwo)
        {

            await lastDiceRollRepository.UpdateManyAsync(
                new LastDiceRollUpdateParams { UtilityDiceOne = diceOne, UtilityDiceTwo = diceTwo },
                new LastDiceRollWhereParams { GameId = gameId },
                new { }
            );

            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToGroup("game:update", game);
        }
        //=======================================================
        // Trade
        //=======================================================
        public async Task TradeCreate(
            Guid gameId,
            Guid initiator,
            PlayerTradeOffer playerOneOffer,
            PlayerTradeOffer playerTwoOffer
        )
        {
            var createParams = new TradeCreateParams
            {
                GameId = gameId,
                Initiator = initiator,
                PlayerOne = playerOneOffer,
                PlayerTwo = playerTwoOffer,
            };
            await tradeRepository.CreateFullTradeAsync(createParams);
            var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
            await SendToGroup("trade:update", trades);
        }
        public async Task TradeSearch(Guid gameId)
        {
            var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
            await SendToSelf("trade:list", trades);
        }
        public async Task TradeUpdate(
            int tradeId,
            Guid updatedBy,
            PlayerTradeOffer playerOneOffer,
            PlayerTradeOffer playerTwoOffer
        )
        {
            var updateparams = new TradeUpdateParams
            {
                LastUpdatedBy = updatedBy,
                PlayerOne = playerOneOffer,
                PlayerTwo = playerTwoOffer,
            };
            await tradeRepository.UpdateFullTradeAsync(tradeId,updateparams);
            var socketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if (socketPlayer.GameId is Guid gameId)
            {
                var trades = await tradeRepository.GetActiveFullTradesForGameAsync(gameId);
                await SendToGroup("trade:update", trades);
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
                await SendToGroup("trade:update", trades);
            }
        }
    }
}

