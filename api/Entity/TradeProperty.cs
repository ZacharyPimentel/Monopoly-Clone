public class TradeProperty
{
    public int Id { get; set; }
    public int GamePropertyId { get; set; }
    public int TradeId { get; set; }
    //joined fields
    public bool Mortgaged { get; set; }
    public required string PropertyName { get; set; }
}