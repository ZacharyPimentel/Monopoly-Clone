public class Player
{
    public required string Id { get; set;}
    public required string PlayerName { get; set;}
    public required int IconId { get; set;}
    public bool Active { get; set;} = true;
    public int Money { get; set;}
    public int BoardSpaceId { get; set;}
    public bool IsReadyToPlay { get; set;} = false;
    public bool InCurrentGame { get; set;} = false;
    public int RollCount { get; set;} = 0;
    
    //Joined properties from PlayerIcon
    public required string IconUrl { get; set;}
}