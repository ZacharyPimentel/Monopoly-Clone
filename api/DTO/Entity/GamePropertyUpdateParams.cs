namespace api.DTO.Entity;
public class GamePropertyUpdateParams
{
    public int? UpgradeCount { get; set; }
    public Guid? PlayerId { get; set; }
    public bool? Mortgaged { get; set; }
}