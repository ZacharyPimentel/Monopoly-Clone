using api.Interface;

namespace api.Interface;
public class GamePropertyUpdateParams
{
    public int? UpgradeCount { get; set; }
    public Guid? PlayerId { get; set; }
    public bool? Mortgaged { get; set; }
}

public interface IGamePropertyRepository : IBaseRepository<GameProperty, int>
{
    Task<bool> CreateForNewGameAsync(Guid gameId);
}