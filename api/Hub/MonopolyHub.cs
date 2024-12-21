namespace api.hub
{
    using System.Data;
    using Dapper;
    using Microsoft.AspNetCore.SignalR;
    public class MonopolyHub(
        GameState<MonopolyHub> gameState,
        IPlayerRepository playerRepository,
        IGameRepository gameRepository,
        IPropertyRepository propertyRepository,
        IGamePropertyRepository gamePropertyRepository,
        IGameLogRepository gameLogRepository,
        ITradeRepository tradeRepository,
        IDbConnection db
    ) : Hub{
        //=======================================================
        // Default socket methods for connect / disconnect
        //=======================================================
        public override Task OnConnectedAsync()
        {
            var newPlayer = new SocketPlayer{SocketId = Context.ConnectionId};
            gameState.Players.Add(newPlayer);
            return base.OnConnectedAsync();
        }
        public override async Task<Task> OnDisconnectedAsync(Exception? exception)
        {
            var currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            //if the socket player has a game player id, update to inactive
            if(currentSocketPlayer.PlayerId != null && currentSocketPlayer.GameId != null)
            {
                await playerRepository.Update(currentSocketPlayer.PlayerId, new PlayerUpdateParams {Active = false});
                var groupPlayers = await playerRepository.Search( new PlayerWhereParams {GameId = currentSocketPlayer.GameId});
                var gamePlayer = groupPlayers.First(x => x.Id == currentSocketPlayer.PlayerId);
                await gameLogRepository.CreateLog(gamePlayer.GameId, $"{gamePlayer.PlayerName} has disconnected.");
                var latestLogs = await gameLogRepository.GetLatestFive(gamePlayer.GameId);
                await SendToGroup("player:updateGroup", groupPlayers);
                await SendToGroup("gameLog:update",latestLogs);
            }
            gameState.RemovePlayer(Context.ConnectionId);
            return base.OnDisconnectedAsync(exception);
        }
        //=======================================================
        // Methods for message delivery (self, group, or all)
        //=======================================================
        private async Task SendToSelf(string eventName, object? data)
        {
            await Clients.Caller.SendAsync(eventName,data);
        }
        private async Task SendToGroup(string eventName, object data)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if(currentSocketPlayer.GameId != null)
            {
                await Clients.Group(currentSocketPlayer.GameId).SendAsync(eventName, data);
            }else{
                throw new Exception("Tried to send data to a group where the GameId was not found.");
            }

            await Clients.Caller.SendAsync(eventName,data);
        }
        private async Task SendToAll(string eventName, object data)
        {
            await Clients.All.SendAsync(eventName, data);
        }

        //=======================================================
        // Player 
        //=======================================================
        public async Task PlayerReconnect(string playerId)
        {
            var socketPlayer = gameState.GetPlayer(Context.ConnectionId);
            await playerRepository.Update(playerId, new PlayerUpdateParams {Active = true});
            socketPlayer.PlayerId = playerId;
            var allPlayers = await playerRepository.GetAllAsync();

            var currentPlayer = allPlayers.First(x => x.Id == playerId);
            await gameLogRepository.CreateLog(currentPlayer.GameId, $"{currentPlayer.PlayerName} has reconnected.");
            var latestLogs = await gameLogRepository.GetLatestFive(currentPlayer.GameId);
            await SendToGroup("gameLog:update",latestLogs);
            await SendToSelf("player:update", socketPlayer);
            await SendToGroup("player:updateGroup",allPlayers);

            //trigger updated player counts in lobby
            var games = await gameRepository.Search(new GameWhereParams{});
            await SendToAll("game:updateAll",games);
        }
        public async Task PlayerCreate(PlayerCreateParams playerCreateParams)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            Player newPlayer = await playerRepository.Create(playerCreateParams);
            currentSocketPlayer.PlayerId = newPlayer.Id;
            var groupPlayers = await playerRepository.Search(new PlayerWhereParams {
                GameId = currentSocketPlayer.GameId
            });

            await gameLogRepository.CreateLog(newPlayer.GameId, $"{newPlayer.PlayerName} has joined the game.");
            var latestLogs = await gameLogRepository.GetLatestFive(newPlayer.GameId);
            await SendToGroup("gameLog:update",latestLogs);
            await SendToSelf("player:update",currentSocketPlayer);
            await SendToGroup("player:updateGroup",groupPlayers);

            //trigger updated player counts in lobby
            var games = await gameRepository.Search(new GameWhereParams{});
            await SendToAll("game:updateAll",games);
        }
        public async Task PlayerUpdate(string playerId,PlayerUpdateParams playerUpdateParams)
        {
            await playerRepository.Update(playerId, playerUpdateParams);
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if(currentSocketPlayer.GameId == null)
            {
                throw new Exception("Player does not have a GameId when it should be there.");
            }

            Game currentGame = await gameRepository.GetByIdAsync(currentSocketPlayer.GameId);
            var activeGroupPlayers = await playerRepository.Search( new PlayerWhereParams{
                Active = true,
                GameId = currentGame.Id
            });

            //if at least two players are all ready and the game is in lobby, start the game
            if (currentGame.InLobby && activeGroupPlayers.All(x => x.IsReadyToPlay == true) && activeGroupPlayers.AsList().Count >= 2)
            {
                await gameRepository.Update(currentGame.Id, new GameUpdateParams{
                    InLobby = false,
                    GameStarted = true
                });
                await playerRepository.UpdateMany(
                    new PlayerWhereParams {Active = true},
                    new PlayerUpdateParams {
                        InCurrentGame = true,
                        IsReadyToPlay = false,
                        Money = currentGame.StartingMoney
                    }
                );

                //randomize and set the turn order
                Random random = new();
                var shuffledActivePlayers = activeGroupPlayers.OrderBy(x => random.Next()).ToArray();
                foreach(var (player,index) in shuffledActivePlayers.Select( (value,i) => (value,i)))
                {
                    var addTurnOrder = @"
                        INSERT INTO TurnOrder (Id,PlayerId,GameId,PlayOrder)
                        VALUES (@Id,@PlayerId, @GameId,@PlayOrder)
                    ";

                    var parameters = new TurnOrder
                    {
                        Id = Guid.NewGuid().ToString(),
                        PlayerId = player.Id,
                        GameId = currentGame.Id,
                        PlayOrder = index + 1
                    };

                    await db.ExecuteAsync(addTurnOrder,parameters);
                } 
                
                var updatedGame = await gameRepository.GetByIdAsync(currentGame.Id);
                await SendToGroup("game:update",updatedGame);
            }
            var updatedGroupPlayers = await playerRepository.Search(new PlayerWhereParams {
                GameId = currentSocketPlayer.GameId
            });
            
            await SendToGroup("player:updateGroup",updatedGroupPlayers);
        }
        
        //=======================================================
        // Game
        //=======================================================
        public async Task GameGetAll()
        {
            var games = await gameRepository.Search(new GameWhereParams{});
            await SendToSelf("game:updateAll", games);
        }
        public async Task GameGetById(string gameId)
        {
            Game game = await gameRepository.GetByIdAsync(gameId);
            await SendToSelf("game:update",game);
        }
        public async Task GameCreate(GameCreateParams gameCreateParams)
        {
            var newGame = await gameRepository.Create(gameCreateParams);
            var properties = await propertyRepository.GetAll();

            //create GameProperty entries
            var gamePropertyInsert = @"
                INSERT INTO GAMEPROPERTY (GameId,PropertyId)
                VALUES (@GameId,@PropertyId)
            ";
            var gameProperties = new List<GameProperty>{};
            foreach (var property in properties)
            {
                gameProperties.Add( new GameProperty {
                    GameId = newGame.Id,
                    PropertyId = property.Id
                });
            }
            await db.ExecuteAsync(gamePropertyInsert,gameProperties);

            await SendToSelf("game:create",newGame.Id);
            var games = await gameRepository.Search(new GameWhereParams{});
            await SendToAll("game:updateAll",games);
        }
        public async Task GameJoin(string gameId)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            currentSocketPlayer.GameId = gameId;
            await Groups.AddToGroupAsync(Context.ConnectionId,gameId);
            Game? game = await gameRepository.GetByIdAsync(gameId);
            if(game == null)
            {
                await SendToSelf("game:update",game);
                return;
            }
            var groupPlayers = await playerRepository.Search(new PlayerWhereParams {GameId = gameId});
            var latestLogs = await gameLogRepository.GetLatestFive(game.Id);
            var trades = await tradeRepository.Search(gameId);
            Console.WriteLine(trades);
            await SendToSelf("game:update",game);
            await SendToSelf("player:update",currentSocketPlayer);
            await SendToSelf("player:updateGroup",groupPlayers);
            await SendToSelf("gameLog:update",latestLogs);
            await SendToSelf("trade:update",trades);
            await BoardSpaceGetAll(gameId);
        }
        public async Task GameLeave(string gameId)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if(currentSocketPlayer.PlayerId != null && currentSocketPlayer.GameId != null)
            {
                await playerRepository.Update(currentSocketPlayer.PlayerId, new PlayerUpdateParams {Active = false});
                var groupPlayers = await playerRepository.Search( new PlayerWhereParams {GameId = currentSocketPlayer.GameId});
                await Groups.RemoveFromGroupAsync(Context.ConnectionId,gameId);
                await SendToGroup("player:updateGroup", groupPlayers);
                currentSocketPlayer.GameId = null;
                currentSocketPlayer.PlayerId = null;
                var games = await gameRepository.Search(new GameWhereParams{});
                await SendToAll("game:updateAll",games);
            }
        }
        public async Task GameUpdate(string gameId,GameUpdateParams gameUpdateParams)
        {
                await gameRepository.Update(gameId,gameUpdateParams);
                Game game = await gameRepository.GetByIdAsync(gameId);
                await SendToGroup("game:Update",game);
        }
        public async Task GameEndTurn()
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);

            if(currentSocketPlayer.PlayerId == null || currentSocketPlayer.GameId == null)
            {
                throw new Exception("Socket player is missing data, PlayerId or GameId");
            }

            var gameId = currentSocketPlayer.GameId;
            Game currentGame = await gameRepository.GetByIdAsync(gameId);

            var markPlayerAsPlayed = @"
                UPDATE TurnOrder
                SET 
                    HasPlayed = TRUE
                WHERE
                    GameId = @GameId AND PlayerId = @PlayerId
            ";
            var parameters = new
            {
                currentSocketPlayer.PlayerId,
                GameId = currentGame.Id,
            };
            await db.ExecuteAsync(markPlayerAsPlayed,parameters);

            //check if everyone has taken their turn, reset if so
            var notPlayedCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM TurnOrder WHERE HasPlayed = FALSE");
            if(notPlayedCount  == 0)
            {
                await db.ExecuteAsync("UPDATE TurnOrder Set HasPlayed = FALSE");
                await playerRepository.UpdateMany(
                    new PlayerWhereParams {InCurrentGame = true},
                    new PlayerUpdateParams { RollCount = 0}
                );
            }

            await playerRepository.Update(currentSocketPlayer.PlayerId, new PlayerUpdateParams{TurnComplete = false});

            var groupPlayers = await playerRepository.Search(new PlayerWhereParams {GameId = gameId});
            var updatedGame = await gameRepository.GetByIdAsync(gameId);
            await SendToGroup("player:updateGroup",groupPlayers);
            await SendToGroup("game:update",updatedGame);
        }
        public async Task BoardSpaceGetAll(string gameId)
        {
            Game game = await db.QuerySingleAsync<Game>("SELECT * FROM Game WHERE Id = @GameId", new {GameId = gameId});
            
            var sql = @"
                SELECT bs.*, bst.BoardSpaceName
                FROM BoardSpace bs
                LEFT JOIN BoardSpaceTheme bst ON bs.Id = bst.BoardSpaceId
                WHERE ThemeId = @ThemeId;

                SELECT 
                    p.*, 
                    gp.Id AS GamePropertyId, 
                    gp.PlayerId, 
                    gp.UpgradeCount, 
                    gp.Mortgaged, 
                    gp.GameId,
                    tp.ThemeId,
                    tp.PropertyId,
                    tp.SetNumber,
                    tp.Color
                FROM Property p
                JOIN GameProperty gp ON p.Id = gp.PropertyId AND gp.GameId = @GameId
                LEFT JOIN ThemeProperty tp ON p.Id = tp.PropertyId AND tp.ThemeId = @ThemeId;

                SELECT * FROM PropertyRent;
            ";

            var multi = await db.QueryMultipleAsync(sql, new { GameId = gameId, game.ThemeId });
            var boardSpaces = multi.Read<BoardSpace>().ToList();
            var properties = multi.Read<Property>().ToList();
            var propertyRents = multi.Read<PropertyRent>().ToList();
            // Map properties to board spaces
            foreach (var property in properties)
            {
                var boardSpace = boardSpaces.FirstOrDefault(bs => bs.Id == property.BoardSpaceId);
                if (boardSpace != null)
                    boardSpace.Property = property;
            }

            // Map property rents to properties
            foreach (var rent in propertyRents)
            {
                var property = properties.FirstOrDefault(p => p.Id == rent.PropertyId);
                property?.PropertyRents.Add(rent);
            }

            await SendToSelf("boardSpace:update",boardSpaces);
        }

        //=======================================================
        // Property
        //=======================================================
        public async Task GamePropertyUpdate(int gamePropertyId, GamePropertyUpdateParams updateParams)
        {
            await gamePropertyRepository.Update(gamePropertyId,updateParams);
            GameProperty gameProperty = await gamePropertyRepository.GetByIdAsync(gamePropertyId);
            Game game = await db.QuerySingleAsync<Game>("SELECT * FROM Game WHERE Id = @GameId", new {gameProperty.GameId});
            
            var sql = @"
                SELECT bs.*, bst.BoardSpaceName
                FROM BoardSpace bs
                LEFT JOIN BoardSpaceTheme bst ON bs.Id = bst.BoardSpaceId
                WHERE ThemeId = @ThemeId;

                SELECT 
                    p.*, 
                    gp.Id AS GamePropertyId, 
                    gp.PlayerId, 
                    gp.UpgradeCount, 
                    gp.Mortgaged, 
                    gp.GameId,
                    tp.ThemeId,
                    tp.PropertyId,
                    tp.SetNumber,
                    tp.Color
                FROM Property p
                JOIN GameProperty gp ON p.Id = gp.PropertyId AND gp.GameId = @GameId
                LEFT JOIN ThemeProperty tp ON p.Id = tp.PropertyId AND tp.ThemeId = @ThemeId;

                SELECT * FROM PropertyRent;
            ";

            var multi = await db.QueryMultipleAsync(sql, new { gameProperty.GameId, game.ThemeId });
            var boardSpaces = multi.Read<BoardSpace>().ToList();
            var properties = multi.Read<Property>().ToList();
            var propertyRents = multi.Read<PropertyRent>().ToList();
            // Map properties to board spaces
            foreach (var property in properties)
            {
                var boardSpace = boardSpaces.FirstOrDefault(bs => bs.Id == property.BoardSpaceId);
                if (boardSpace != null)
                    boardSpace.Property = property;
            }

            // Map property rents to properties
            foreach (var rent in propertyRents)
            {
                var property = properties.FirstOrDefault(p => p.Id == rent.PropertyId);
                property?.PropertyRents.Add(rent);
            }
            await SendToGroup("boardSpace:update",boardSpaces);
        }

        //=======================================================
        // GameLog
        //=======================================================
        public async Task GameLogCreate(string gameId, string message)
        {
            await gameLogRepository.CreateLog(gameId,message);
            var latestLogs = await gameLogRepository.GetLatestFive(gameId);
            await SendToGroup("gameLog:update",latestLogs);
        }

        //=======================================================
        // Last Dice Roll
        //=======================================================
        public async Task LastDiceRollUpdate(string gameId,int diceOne, int diceTwo)
        {
            var sql = @"
                UPDATE LASTDICEROLL
                SET
                    DiceOne = @DiceOne,
                    DiceTwo = @DiceTwo
                WHERE GameId = @GameId
            ";

            await db.ExecuteAsync(sql, new {
                GameId = gameId,
                DiceOne = diceOne,
                DiceTwo = diceTwo
            });

            Game game = await gameRepository.GetByIdAsync(gameId);
            await SendToGroup("game:update", game);
        }
        //=======================================================
        // Trade
        //=======================================================
        public async Task TradeCreate(
            string gameId,
            PlayerTradeCreateParams playerOneOffer,
            PlayerTradeCreateParams playerTwoOffer
        ){

            var createParams =  new TradeCreateParams {
                GameId = gameId,
                PlayerOne = playerOneOffer,
                PlayerTwo = playerTwoOffer,
            };
            await tradeRepository.Create(createParams);
            var trades = await tradeRepository.Search(gameId);
            await SendToGroup("trade:update",trades);
        }
        public async Task TradeSearch(string gameId)
        {
            var trades = await tradeRepository.Search(gameId);
            await SendToSelf("trade:list",trades);
        }
        public async Task TradeUpdate(
            int tradeId,
            PlayerTradeCreateParams playerOneOffer,
            PlayerTradeCreateParams playerTwoOffer
        ){
            var updateparams = new TradeUpdateParams{
                TradeId = tradeId,
                PlayerOne = playerOneOffer,
                PlayerTwo = playerTwoOffer,
            };
            await tradeRepository.Update(updateparams);
            var socketPlayer = gameState.GetPlayer(Context.ConnectionId);
            var trades = await tradeRepository.Search(socketPlayer.GameId);
            await SendToGroup("trade:update",trades);
        }
    }
}

