public class PropertyUpdateParams
{
    public int? UpgradeCount { get; set; }
    public string? PlayerId { get; set; }
    public bool? Mortgaged { get; set; }
}

public interface IPropertyRepository
{
    Task<Property> GetByIdAsync(int propertyId);
    Task<bool> Update(int id,PropertyUpdateParams updateParams);
}