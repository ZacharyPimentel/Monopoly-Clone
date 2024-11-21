public class PropertyUpdateParams
{
    public int? UpgradeCount { get; set; }
    public string? PlayerId { get; set; }
    public bool? Mortgaged { get; set; }
}

public interface IPropertyRepository
{
    Task<IEnumerable<Property>> GetAll();
    Task<Property> GetByIdAsync(int gamePropertyId);
}