using api.Entity;
using api.Interface;
public class PropertyUpdateParams
{
    public int? UpgradeCount { get; set; }
    public string? PlayerId { get; set; }
    public bool? Mortgaged { get; set; }
}

public interface IPropertyRepository : IBaseRepository<Property,int>
{
    
}