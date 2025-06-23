using api.DTO.Entity;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Interface;

namespace api.Service.GameLogic;

public interface ITradeService
{
    public Task CreateGameTrade(TradeCreateParams tradeCreateParams);
    public Task DeclineTrade(Player playerId, int tradeId);
}
public class TradeService(
    ITradeRepository tradeRepository,
    IPlayerTradeRepository playerTradeRepository,
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
    public async Task DeclineTrade(Player player, int tradeId)
    {
        IEnumerable<PlayerTrade> playerTrades = await playerTradeRepository.SearchAsync(new PlayerTradeWhereParams
        {
            TradeId = tradeId
        },
        new { }
        );
        if (!playerTrades.Any(pt => pt.PlayerId == player.Id))
        {
            string errorMessage = EnumExtensions.GetEnumDescription(Errors.PlayerCannotModifyTrade);
            throw new Exception(errorMessage);
        }
        await tradeRepository.UpdateAsync(tradeId, new TradeUpdateParams
        {
            DeclinedBy = player.Id
        });
        await socketMessageService.SendGameStateUpdate(player.GameId,new GameStateIncludeParams
        {
            Trades = true
        });
    }

}