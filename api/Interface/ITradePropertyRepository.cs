using api.Entity;
using api.Interface;
namespace api.Interface;

public class TradePropertyCreateParams
{
    public required int PlayerTradeId { get; set; }
    public required int GamePropertyId { get; set; }
}   
public interface ITradePropertyRepository : IBaseRepository<TradeProperty, int>
{
    Task<List<TradeProperty>> GetAllForPlayerTradeWithPropertyDetailsAsync(int PlayerTradeId);
}