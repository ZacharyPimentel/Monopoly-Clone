namespace api.Entity;
public class PlayerDebt
{
    public int Id { get; set; }
    public Guid PlayerId { get; set; }
    public int Amount { get; set; }
    public Guid? InDebtTo { get; set; }
    public bool DebtPaid { get; set; }
}