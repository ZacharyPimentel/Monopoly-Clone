public class Trade
{
    public int Id { get; set;}
    public required string GameId { get; set;}
    //joined fields
    public List<PlayerTrade> PlayerTrades{ get; set;} = [];
}
