public class GamePropertyUpdateParams
{
    public int? UpgradeCount { get; set; }
    public string? PlayerId { get; set; }
    public bool? Mortgaged { get; set; }
}

public interface IGamePropertyRepository
{
    Task<GameProperty> GetByIdAsync(int gamePropertyId);
    Task<bool> Update(int gamePropertyId ,GamePropertyUpdateParams updateParams);
}