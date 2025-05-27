public class Trade
{
    public int Id { get; set;}
    public required string GameId { get; set;}
    public required string InitiatedBy { get; set;}
    public required string LastUpdatedBy { get; set;}
    public string? DeclinedBy { get; set; }
    public string? AcceptedBy {get; set;}
    //joined fields
    public List<PlayerTrade> PlayerTrades { get; set; } = [];
}
