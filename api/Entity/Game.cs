public class Game
{
    public required string Id { get; set; }
    public required string GameName { get; set; }
    public bool InLobby { get; set; }
    public bool GameOver { get; set; }
    public bool GameStarted { get; set; }
    public int StartingMoney { get; set; }
    public int DiceOneLastRoll { get; set; }
    public int DiceTwoLastRoll { get; set; }
    //Join for PlayerId on TurnOrder with GameId
    public string? CurrentPlayerTurn { get; set; }
    //Join for game PlayerId on Game when searching games
    public int? ActivePlayerCount { get; set; } = 0;
    public LastDiceRoll? LastDiceRoll { get; set; }
}