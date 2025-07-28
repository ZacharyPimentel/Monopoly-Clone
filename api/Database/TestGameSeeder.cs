using System.Data;
using api.DTO.Entity;
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
    }

    private async Task<Game> SeedGame(string gameName)
    {
        await gameService.CreateGame(new GameCreateParams { GameName = gameName, ThemeId = 1 });
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
        return players;
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

        var cards = (await cardRepository.GetAllAsync()).Where( c => c.CardActionId == (int)CardActionIds.PayPlayers);
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
}