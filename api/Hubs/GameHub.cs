// using System.Data;
// using System.Data.Common;
// using Dapper;
// using Microsoft.AspNetCore.SignalR;

// namespace SignalRWebpack.Hubs;

// class GameHub(
//     GameState<GameHub> gameState,
//     IDbConnection db,
//     IPlayerRepository playerRepository,
//     IGameRepository gameRepository,
//     IPropertyRepository propertyRepository
// ) : Hub{

//     public async override Task<Task> OnConnectedAsync()
//     {
        
//         var newPlayer = new SocketPlayer{SocketId = Context.ConnectionId};
//         gameState.Players.Add(newPlayer);
//         await Clients.Caller.SendAsync("UpdateCurrentPlayer", newPlayer);

//         var allPlayers = await playerRepository.GetAllAsync();

//         var game = await gameRepository.GetByIdAsync(1);
//         await Clients.Caller.SendAsync("UpdateGameState", game);
//         await Clients.All.SendAsync("UpdatePlayers", allPlayers);
//         await Clients.All.SendAsync("UpdateLastDiceRoll",gameState.LastDiceRoll);
//         return base.OnConnectedAsync();
//     }

//     public override async Task<Task> OnDisconnectedAsync(Exception? exception)
//     {
        
//         var currentPlayer = gameState.GetPlayer(Context.ConnectionId);

//         //if the socket player has a game player id, update to inactive
//         if(currentPlayer.PlayerId != null)
//         {
//             await playerRepository.Update(currentPlayer.PlayerId, new PlayerUpdateParams {Active = false});
//         }

//         var allPlayers = await playerRepository.GetAllAsync();
//         gameState.RemovePlayer(Context.ConnectionId);
//         await Clients.All.SendAsync("UpdatePlayers", allPlayers);
//         return base.OnDisconnectedAsync(exception);
//     }

//     public async Task ReconnectPlayer(string playerId)
//     {
//         var currentPlayer = gameState.GetPlayer(Context.ConnectionId);
//         currentPlayer.PlayerId = playerId;
//         await playerRepository.Update(playerId, new PlayerUpdateParams {Active = true});
//         var allPlayers = await playerRepository.GetAllAsync();

//         await Clients.All.SendAsync("UpdatePlayers",allPlayers);
//         await Clients.Caller.SendAsync("UpdateCurrentPlayer", currentPlayer);
//     }

//     public async Task AddNewPlayer(string name, int iconId)
//     {
//         SocketPlayer currentPlayer = gameState.GetPlayer(Context.ConnectionId);

//         var newPlayer = await playerRepository.Create(new PlayerCreateParams
//         {
//             PlayerName = name,
//             IconId = iconId,
//         });

//         currentPlayer.PlayerId = newPlayer.Id;

//         var allPlayers = await playerRepository.GetAllAsync();

//         await Clients.Caller.SendAsync("UpdateCurrentPlayer",currentPlayer);
//         await Clients.All.SendAsync("UpdatePlayers",allPlayers);
//     }

//     public async Task UpdatePlayer(string playerId, PlayerUpdateParams playerUpdateParams)
//     {
//         await playerRepository.Update(playerId, playerUpdateParams);
//         var allPlayers = await playerRepository.GetAllAsync();
//         var activePlayers = allPlayers.Where(x => x.Active).ToList();
//         var currentGame = await gameRepository.GetByIdAsync(1);
        
//         await Clients.All.SendAsync("UpdatePlayers",allPlayers);
        
//         //if at least two players are all ready and the game is in lobby, start the game
//         if (currentGame.InLobby && activePlayers.All(x => x.IsReadyToPlay == true) && activePlayers.Count >= 2)
//         {
//             await gameRepository.Update(currentGame.Id, new GameUpdateParams{
//                 InLobby = false,
//                 GameStarted = true
//             });
//             await playerRepository.UpdateMany(
//                 new PlayerWhereParams {Active = true},
//                 new PlayerUpdateParams {
//                     InCurrentGame = true,
//                     IsReadyToPlay = false,
//                     Money = currentGame.StartingMoney
//                 }
//             );

//             //randomize and set the turn order
//             Random random = new();
//             var shuffledActivePlayers = activePlayers.OrderBy(x => random.Next()).ToArray();
//             foreach(var (player,index) in shuffledActivePlayers.Select( (value,i) => (value,i)))
//             {
//                 var addTurnOrder = @"
//                     INSERT INTO TurnOrder (Id,PlayerId,GameId,PlayOrder)
//                     VALUES (@Id,@PlayerId, @GameId,@PlayOrder)
//                 ";

//                 var parameters = new TurnOrder
//                 {
//                     Id = index + 1,
//                     PlayerId = player.Id,
//                     GameId = currentGame.Id,
//                     PlayOrder = index + 1
//                 };

//                 await db.ExecuteAsync(addTurnOrder,parameters);
//             }

//             var updatedGame = await gameRepository.GetByIdAsync(1);
//             var updatedPlayers = await playerRepository.GetAllAsync();

//             await Clients.All.SendAsync("UpdateGameState",updatedGame);
//             await Clients.All.SendAsync("UpdatePlayers",updatedPlayers);
//         }
//     }
//     public async Task UpdateRules(int gameId, GameUpdateParams gameUpdateParams)
//     {
//         await gameRepository.Update(gameId,gameUpdateParams);
//         var updatedGame = await gameRepository.GetByIdAsync(gameId);
//         await Clients.All.SendAsync("UpdateGameState",updatedGame);
//     }

//     public async Task SetLastDiceRoll(int[] rolls)
//     {
//         gameState.SetLastDiceRoll(rolls);
//         await Clients.All.SendAsync("UpdateLastDiceRoll",rolls);
//     }
//     public async Task EndTurn(int gameId)
//     {
//         SocketPlayer currentSocketPlayer = gameState.GetPlayer(Context.ConnectionId);
//         Game currentGame = await gameRepository.GetByIdAsync(gameId);

//         await playerRepository.Update(currentSocketPlayer.PlayerId, new PlayerUpdateParams{TurnComplete = false});

//         var markPlayerAsPlayed = @"
//             UPDATE TurnOrder
//             SET 
//                 HasPlayed = TRUE
//             WHERE
//                 GameId = @GameId AND PlayerId = @PlayerId
//         ";
//         var parameters = new
//         {
//             currentSocketPlayer.PlayerId,
//             GameId = currentGame.Id,
//         };
//         await db.ExecuteAsync(markPlayerAsPlayed,parameters);

//         //check if everyone has taken their turn, reset if so
//         var notPlayedCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM TurnOrder WHERE HasPlayed = FALSE");
//         if(notPlayedCount  == 0)
//         {
//             await db.ExecuteAsync("UPDATE TurnOrder Set HasPlayed = FALSE");
//             await playerRepository.UpdateMany(
//                 new PlayerWhereParams {InCurrentGame = true},
//                 new PlayerUpdateParams { RollCount = 0}
//             );
//         }

//         var allPlayers = await playerRepository.GetAllAsync();
//         var updatedGame = await gameRepository.GetByIdAsync(gameId);
//         await Clients.All.SendAsync("UpdatePlayers",allPlayers);
//         await Clients.All.SendAsync("UpdateGameState",updatedGame);
//     }

//     public async Task UpdateProperty(int propertyId, PropertyUpdateParams updateParams)
//     {
//         await propertyRepository.Update(propertyId,updateParams);
        
//         var sql = @"
//             SELECT * FROM BoardSpace;
//             SELECT * FROM Property;
//             SELECT * FROM PropertyRent;
//         ";
//         var multi = await db.QueryMultipleAsync(sql);
//         var boardSpaces = multi.Read<BoardSpace>().ToList();
//         var properties = multi.Read<Property>().ToList();
//         var propertyRents = multi.Read<PropertyRent>().ToList();
//         // Map properties to board spaces
//         foreach (var property in properties)
//         {
//             var boardSpace = boardSpaces.FirstOrDefault(bs => bs.Id == property.BoardSpaceId);
//             if (boardSpace != null)
//                 boardSpace.Property = property;
//         }

//         // Map property rents to properties
//         foreach (var rent in propertyRents)
//         {
//             var property = properties.FirstOrDefault(p => p.Id == rent.PropertyId);
//             if (property != null)
//             {
//                 property.PropertyRents.Add(rent);
//             }
//         }
//         await Clients.All.SendAsync("UpdateBoardSpaces",boardSpaces);
//     }
// }