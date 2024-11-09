public class Game
{
    public int Id { get; set; }
    public bool InLobby { get; set; }
    public bool GameOver { get; set; }
    public bool GameStarted { get; set; }
    public int StartingMoney { get; set; }
    //Join for PlayerId on TurnOrder with GameId
    public string? CurrentPlayerTurn { get; set; }
}