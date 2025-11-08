using System.Data;
using api.DTO.Entity;
using api.DTO.Websocket;
using api.Entity;
using api.Enumerable;
using api.Interface;
using api.Repository;
using api.Service;
using api.Service.GameLogic;

namespace api.Database;

public class TestGameSeeder(
        ISocketMessageService socketMessagingService,
        IGameRepository gameRepository,
        IGamePropertyRepository gamePropertyRepository,
        IPlayerRepository playerRepository,
        ISpaceLandingService spaceLandingService,
        IPlayerService playerService,
        IGameService gameService,
        ITurnOrderRepository turnOrderRepository,
        IGameCardRepository gameCardRepository,
        ICardRepository cardRepository
)
{
    public async Task SeedTestingGameData()
    {
        socketMessagingService.SuppressMessages = true;

        // Game 1: Single player can't pay / bankruptcy test
        Game game1 = await SeedGame("Cannot pay / bankruptcy test");
        IEnumerable<Player> game1Players = await SeedPlayers(game1.Id, 2);
        await SeedGameStart(game1, game1Players);
        await SeedBankruptcyTest(game1, game1Players);

        // Game 2: Single player can't pay / bankruptcy test
        Game game2 = await SeedGame("Cannot pay multiple / bankruptcy test");
        IEnumerable<Player> game2Players = await SeedPlayers(game2.Id, 3);
        await SeedGameStart(game2, game2Players);
        await SeedMultipleDebtTest(game2, game2Players);

        // Game 3: Player lands on go, other player passes GO
        Game game3 = await SeedGame("GO logic test");
        IEnumerable<Player> game3Players = await SeedPlayers(game3.Id, 2);
        game3.ExtraMoneyForLandingOnGo = true;
        await gameRepository.UpdateAsync(game3.Id, new GameUpdateParams { ExtraMoneyForLandingOnGo = true });
        await SeedGameStart(game3, game3Players);
        await SeedLandedOnGoTest(game3, game3Players);

        // Game 4: Player lands on chance - Advance to GO
        Game game4 = await SeedGame("Advance to GO test");
        IEnumerable<Player> game4Players = await SeedPlayers(game4.Id, 2);
        game4.ExtraMoneyForLandingOnGo = true;
        await gameRepository.UpdateAsync(game4.Id, new GameUpdateParams { ExtraMoneyForLandingOnGo = true });
        await SeedGameStart(game4, game4Players);
        await SeedAdvanceToGoTest(game4, game4Players);

        //Game 5: Advance to railroad and utility GO passing test
        Game game5 = await SeedGame("Advance to RR / Util Passing GO");
        IEnumerable<Player> game5Players = await SeedPlayers(game5.Id, 2);
        await SeedGameStart(game5, game5Players);
        await SeedAdvanceToRailroadUtilityTest(game5, game5Players);

        //Game 6: Roll for utilities  
        Game game6 = await SeedGame("Test rolling for utilities");
        IEnumerable<Player> game6Players = await SeedPlayers(game6.Id, 2);
        await SeedGameStart(game6, game6Players);
        await SeedRollForUtilitiesTest(game6, game6Players);

        //Game 7: Jail testing 
        Game game7 = await SeedGame("Test jail");
        IEnumerable<Player> game7Players = await SeedPlayers(game7.Id, 2);
        await SeedGameStart(game7, game7Players);
        await SeedJailTest(game7, game7Players);
    }

    private async Task<Game> SeedGame(string gameName)
    {
        var createParams = new GameCreateParams
        {
            GameName = gameName,
            ThemeId = 1
        };
        await gameService.CreateGame(new SocketEventGameCreate { GameCreateParams = createParams });
        var game = (await gameRepository.Search(new GameWhereParams { GameName = gameName }))
            .First() ?? throw new Exception("Game is null in SeedGame");

        return game;
    }
    private async Task<IEnumerable<Player>> SeedPlayers(Guid gameId, int numberOfPlayers)
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            await playerRepository.CreateAsync(new PlayerCreateParams
            {
                PlayerName = $"Player {i + 1}",
                IconId = i + 1,
                GameId = gameId,
                Active = false,
                IsReadyToPlay = false
            });
        }
        IEnumerable<Player> players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = gameId }, null);
        return players.OrderBy(p => p.PlayerName);
    }

    private async Task SeedGameStart(Game game, IEnumerable<Player> players)
    {
        foreach (var player in players)
        {
            await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams { Active = true });
        }
        foreach (var player in players)
        {
            await playerService.SetPlayerReadyStatus(player, game, true);
        }
        foreach (var player in players)
        {
            await playerRepository.UpdateAsync(player.Id, new PlayerUpdateParams { Active = false });
        }
    }

    public async Task SeedBankruptcyTest(Game game, IEnumerable<Player> players)
    {
        Player playerOne = players.First();
        Player playerTwo = players.Last();
        await gamePropertyRepository.AssignAllToPlayer(game.Id, playerOne.Id);
        await playerRepository.UpdateAsync(playerTwo.Id, new PlayerUpdateParams
        {
            Money = 0,
            BoardSpaceId = 40
        });
        await turnOrderRepository.UpdateWhereAsync(
            new TurnOrderUpdateParams { HasPlayed = true },
            new TurnOrderWhereParams { PlayerId = playerOne.Id, GameId = game.Id },
            new { }
        );
        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        await gameRepository.UpdateAsync(game.Id,new GameUpdateParams { AllowedToBuildUnevenly = true });
        players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = game.Id }, null);
        await spaceLandingService.HandleLandedOnSpace(players, game);
    }

    public async Task SeedMultipleDebtTest(Game game, IEnumerable<Player> players)
    {
        Player playerOne = players.First();
        await playerRepository.UpdateAsync(playerOne.Id, new PlayerUpdateParams
        {
            Money = 0,
            BoardSpaceId = 8 //Chance
        });

        await turnOrderRepository.UpdateWhereAsync(
            new TurnOrderUpdateParams { HasPlayed = true },
            new TurnOrderWhereParams { GameId = game.Id },
            new TurnOrderWhereParams { PlayerId = playerOne.Id }
        );

        var cards = (await cardRepository.GetAllAsync()).Where(c => c.CardActionId == (int)CardActionIds.PayPlayers);
        var firstMatchingCard = cards.ToList()[0];

        await gameCardRepository.UpdateWhereAsync(
            new GameCardUpdateParams { Played = true },
            new GameCardWhereParams { GameId = game.Id },
            new GameCardWhereParams { CardId = firstMatchingCard.Id }
        );
        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = game.Id }, null);
        await spaceLandingService.HandleLandedOnSpace(players, game);
    }

    public async Task SeedLandedOnGoTest(Game game, IEnumerable<Player> players)
    {
        Player playerOne = players.OrderBy(p => p.PlayerName).First();
        Player playerTwo = players.OrderBy(p => p.PlayerName).Last();
        await playerRepository.UpdateAsync(playerOne.Id, new PlayerUpdateParams
        {
            BoardSpaceId = 38 //Park Place
        });
        await playerRepository.UpdateAsync(playerTwo.Id, new PlayerUpdateParams
        {
            BoardSpaceId = 38 //Park Place
        });

        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = game.Id }, null);
        //roll for turn with forced dice roll to land on GO
        await playerService.RollForTurn(players.OrderBy(p => p.PlayerName).First(), game, 1, 2);
        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        await gameService.EndTurn(playerOne, game);
        // game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = game.Id }, null);
        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        //roll for turn with forced dice roll to land just past GO
        await playerService.RollForTurn(players.OrderBy(p => p.PlayerName).Last(), game, 1, 3);
    }

    public async Task SeedAdvanceToGoTest(Game game, IEnumerable<Player> players)
    {
        Player playerOne = players.OrderBy(p => p.PlayerName).First();
        Player playerTwo = players.OrderBy(p => p.PlayerName).Last();
        await playerRepository.UpdateAsync(playerOne.Id, new PlayerUpdateParams
        {
            BoardSpaceId = 8 //Chance
        });

        await gameCardRepository.UpdateWhereAsync(
            new GameCardUpdateParams { Played = true },
            new GameCardWhereParams { GameId = game.Id, },
            new GameCardWhereParams { CardId = 2 }
        );

        await turnOrderRepository.UpdateWhereAsync(
            new TurnOrderUpdateParams { HasPlayed = true },
            new TurnOrderWhereParams { GameId = game.Id },
            new TurnOrderWhereParams { PlayerId = playerOne.Id }
        );

        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = game.Id }, null);
        await spaceLandingService.HandleLandedOnSpace(players, game, true);
    }

    public async Task SeedAdvanceToRailroadUtilityTest(Game game, IEnumerable<Player> players)
    {
        Player playerOne = players.OrderBy(p => p.PlayerName).First();
        Player playerTwo = players.OrderBy(p => p.PlayerName).Last();
        await playerRepository.UpdateAsync(playerOne.Id, new PlayerUpdateParams
        {
            BoardSpaceId = 37 //Chance
        });

        await gameCardRepository.UpdateWhereAsync(
            new GameCardUpdateParams { Played = true },
            new GameCardWhereParams { GameId = game.Id, },
            new GameCardWhereParams { CardId = 5 } //Advance to Railroad
        );

        await turnOrderRepository.UpdateWhereAsync(
            new TurnOrderUpdateParams { HasPlayed = true },
            new TurnOrderWhereParams { GameId = game.Id },
            new TurnOrderWhereParams { PlayerId = playerOne.Id }
        );

        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = game.Id }, null);
        await spaceLandingService.HandleLandedOnSpace(players, game, true);
    }

    public async Task SeedRollForUtilitiesTest(Game game, IEnumerable<Player> players)
    {
        Player playerOne = players.OrderBy(p => p.PlayerName).First();
        Player playerTwo = players.OrderBy(p => p.PlayerName).Last();
        await playerRepository.UpdateAsync(playerOne.Id, new PlayerUpdateParams
        {
            BoardSpaceId = 11 //Jail
        });

        await turnOrderRepository.UpdateWhereAsync(
            new TurnOrderUpdateParams { HasPlayed = true },
            new TurnOrderWhereParams { GameId = game.Id },
            new TurnOrderWhereParams { PlayerId = playerOne.Id }
        );
        await gamePropertyRepository.AssignAllToPlayer(game.Id, playerTwo.Id);
        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = game.Id }, null);
        await playerService.RollForTurn(players.OrderBy(p => p.PlayerName).First(), game, 1, 1);
    }

    public async Task SeedJailTest(Game game, IEnumerable<Player> players)
    {
        Player playerOne = players.OrderBy(p => p.PlayerName).First();
        Player playerTwo = players.OrderBy(p => p.PlayerName).Last();
        await playerRepository.UpdateAsync(playerOne.Id, new PlayerUpdateParams
        {
            BoardSpaceId = 11, //Jail
            InJail = true,
            GetOutOfJailFreeCards = 1,
            CanRoll = true
        });
        await turnOrderRepository.UpdateWhereAsync(
            new TurnOrderUpdateParams { HasPlayed = true },
            new TurnOrderWhereParams { GameId = game.Id },
            new TurnOrderWhereParams { PlayerId = playerOne.Id }
        );

    }
}