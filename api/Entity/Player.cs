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
    public bool TurnComplete { get; set;} = true;
    public bool InJail { get; set;} = false;
    public required string GameId { get; set;}
    public bool RollingForUtilities { get; set;} = false;
    public int JailTurnCount { get; set;} = 0;
    public int GetOutOfJailFreeCards { get; set;} = 0;
    
    //Joined properties from PlayerIcon
    public required string IconUrl { get; set;}
}