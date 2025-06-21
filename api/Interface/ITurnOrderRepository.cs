using api.Entity;
namespace api.Interface;
public class TurnOrderCreateParams
{
    public required Guid PlayerId { get; set; }
    public required Guid GameId { get; set; }
    public required int PlayOrder { get; set; }
}
public class TurnOrderUpdateParams
{
    public bool? HasPlayed { get; set; }
}
public class TurnOrderWhereParams
{
    public Guid? PlayerId { get; set; }
    public Guid? GameId { get; set; }
}

public interface ITurnOrderRepository : IBaseRepository<TurnOrder, Guid>
{
    public Task<TurnOrder> GetNextTurnByGameAsync(Guid GameId);
}