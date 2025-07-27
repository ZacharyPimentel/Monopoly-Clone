namespace api.DTO.Entity;
public class PlayerDebtCreateParams
{
    public Guid PlayerId { get; set; }
    public Guid? InDebtTo { get; set; }
    public int Amount { get; set; }
}