public class Player
{
    public required string Id { get; set;}
    public required string PlayerName { get; set;}
    public required int IconId { get; set;}
    public bool Active { get; set;} = true;
    public int Money { get; set;}
    public int CurrentBoardSpace { get; set;}
    public bool IsReadyToPlay { get; set;} = false;
    
    //Joined properties from PlayerIcon
    public required string IconUrl { get; set;}
}