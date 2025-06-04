namespace api.hub
{
    using System.Data;
    using api.DTO.Entity;
    using api.DTO.Websocket;
    using api.Entity;
    using api.Enumerable;
    using api.Interface;
    using api.Service;
    using Dapper;
    using Microsoft.AspNetCore.SignalR;
    public class MonopolyHub(
        GameState<MonopolyHub> gameState,
        IDbConnection db,
        IBoardSpaceRepository boardSpaceRepository,
        IGameLogRepository gameLogRepository,
        IGamePropertyRepository gamePropertyRepository,
        IGameRepository gameRepository,
        ILastDiceRollRepository lastDiceRollRepository,
        IPlayerRepository playerRepository,
        ITradeRepository tradeRepository,
        ITurnOrderRepository turnOrderRepository,
        IGameService gameService,
        IPlayerService playerService
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
            var playerTryingToReconnectTo = await playerRepository.GetByIdAsync(playerId);
            if (
                playerTryingToReconnectTo == null ||
                playerTryingToReconnectTo.Active ||
                socketPlayer.GameId != playerTryingToReconnectTo.GameId
            )
            {
                throw new Exception("Error reconnecting to player");
            }
            await playerService.ReconnectToGame(playerId);
        }
        public async Task PlayerCreate(SocketEventPlayerCreate playerCreateParams)
        {
            ValidateGameId(playerCreateParams.GameId);
            await playerService.CreatePlayer(playerCreateParams);
        }
        public async Task PlayerEdit(SocketEventPlayerEdit playerEditParams)
        {
            await playerService.EditPlayer(playerEditParams);
        }
        public async Task PlayerReady(SocketEventPlayerReady playerReadyParams)
        {
            
            await playerService.SetPlayerReadyStatus(playerReadyParams);
        }
        public async Task PlayerRollForTurn()
        {
            await playerService.RollForTurn();
        }
        public async Task PlayerUpdate(SocketEventPlayerUpdate playerUpdateParams)
        {
            SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
            if (currentSocketPlayer.GameId is not Guid gameId)
            {
                throw new Exception("Player does not have a GameId when it should be there.");
            }
            await playerRepository.UpdateAsync(playerUpdateParams.PlayerId, playerUpdateParams.PlayerUpdateParams);

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
            var games = await gameRepository.GetAllWithPlayerCountAsync();
            await SendToSelf(WebSocketEvents.GameUpdateAll, games);
        }
        public async Task GameGetById(Guid gameId)
        {
            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToSelf(WebSocketEvents.GameUpdate, game);
        }
        public async Task GameCreate(GameCreateParams gameCreateParams)
        {
            await gameService.CreateGame(gameCreateParams);
        }
        public async Task GameJoin(Guid gameId)
        {
            await gameService.JoinGame(gameId);
        }
        public async Task GameLeave(Guid gameId)
        {

            await gameService.LeaveGame(gameId);
        }
        public async Task GameUpdate(Guid gameId, GameUpdateParams gameUpdateParams)
        {
            await gameRepository.UpdateAsync(gameId, gameUpdateParams);
            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToGroup(WebSocketEvents.GameUpdate, game);
        }
        public async Task GameEndTurn()
        {
            await gameService.EndTurn();
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

            await lastDiceRollRepository.UpdateWhereAsync(
                new LastDiceRollUpdateParams { DiceOne = diceOne, DiceTwo = diceTwo },
                new LastDiceRollWhereParams { GameId = gameId },
                new { }
            );

            Game? game = await gameRepository.GetByIdWithDetailsAsync(gameId);
            await SendToGroup(WebSocketEvents.GameUpdate, game);
        }
        public async Task LastUtilityDiceRollUpdate(Guid gameId, int? diceOne, int? diceTwo)
        {

            await lastDiceRollRepository.UpdateWhereAsync(
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

