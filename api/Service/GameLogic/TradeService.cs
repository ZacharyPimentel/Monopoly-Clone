using api.DTO.Entity;
using api.Entity;
using api.Interface;

namespace api.Service.GameLogic;

public interface ITradeService
{
    public Task CreateGameTrade(TradeCreateParams tradeCreateParams);
}
public class TradeService(
    ITradeRepository tradeRepository,
    ISocketMessageService socketMessageService
) : ITradeService
{
    public async Task CreateGameTrade(TradeCreateParams createParams)
    {
        await tradeRepository.CreateFullTradeAsync(createParams);
        await socketMessageService.SendGameStateUpdate(createParams.GameId, new GameStateIncludeParams
        {
            Trades = true
        });
    }
}