using System.Data;
using api.DTO.Entity;
using api.Entity;
using api.Interface;
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
        ITurnOrderRepository turnOrderRepository
)
{
    public async Task SeedTestingGameData()
    {
        socketMessagingService.SuppressMessages = true;


        // Game 1: Fresh Game With Two Players
        Game game1 = await SeedGame("Cannot pay / bankruptcy test");
        IEnumerable<Player> players = await SeedDefaultPlayers(game1.Id);
        await SeedGameStart(game1, players);
        await SeedBankruptcyTest(game1, players);


        // await playerService.SetPlayerReadyStatus(game1Player2, game1, true);
        // game1 = await gameRepository.GetByIdWithDetailsAsync(game1.Id);
        // IEnumerable<Player> game1Players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = game1.Id }, null);
        // await spaceLandingService.HandleLandedOnSpace(game1Players, game1);
    }

    private async Task<Game> SeedGame(string gameName)
    {
        await gameService.CreateGame(new GameCreateParams { GameName = gameName, ThemeId = 1 });
        var game = (await gameRepository.Search(new GameWhereParams { GameName = gameName }))
            .First() ?? throw new Exception("Game is null in SeedGame");
        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        return game;
    }
    private async Task<IEnumerable<Player>> SeedDefaultPlayers(Guid gameId)
    {
        await playerRepository.CreateAsync(new PlayerCreateParams
        {
            PlayerName = "Player One",
            IconId = 1,
            GameId = gameId,
            Active = false,
            IsReadyToPlay = false
        });
        await playerRepository.CreateAsync(new PlayerCreateParams
        {
            PlayerName = "Player Two",
            IconId = 2,
            GameId = gameId,
            Active = false
        });
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
            new TurnOrderWhereParams { PlayerId = playerOne.Id },
            new { }
        );
        game = await gameRepository.GetByIdWithDetailsAsync(game.Id);
        players = await playerRepository.SearchWithIconsAsync(new PlayerWhereParams { GameId = game.Id }, null);
        await spaceLandingService.HandleLandedOnSpace(players, game);
    }
}