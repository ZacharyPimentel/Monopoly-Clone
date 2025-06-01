
using api.Enumerable;
using api.Interface;
using api.Service;
namespace api.Hub.Services;
public interface IGameService
{
    Task CreateGame(GameCreateParams gameCreateParams);
}

public class GameService(
    ISocketMessageService socketMessageService,
    IGameRepository gameRepository,
    ILastDiceRollRepository lastDiceRollRepository,
    IGamePropertyRepository gamePropertyRepository,
    IGameCardRepository gameCardRepository
    
) : IGameService
{
    public async Task CreateGame(GameCreateParams gameCreateParams)
    {
        var newGame = await gameRepository.CreateAndReturnAsync(gameCreateParams);
        //populate tables for new game
        await lastDiceRollRepository.CreateAsync(new { GameId = newGame.Id });
        await gamePropertyRepository.CreateForNewGameAsync(newGame.Id);
        await gameCardRepository.CreateForNewGameAsync(newGame.Id);
        await socketMessageService.SendToSelf(WebSocketEvents.GameCreate, newGame.Id);;
        var games = await gameRepository.GetAllAsync();
        await socketMessageService.SendToAll(WebSocketEvents.GameUpdateAll, games);
    }   
}
