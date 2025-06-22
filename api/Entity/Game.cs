using TypeGen.Core.TypeAnnotations;
namespace api.Entity;
[ExportTsInterface]
public class Game
{
    public required Guid Id { get; set; }
    public required string GameName { get; set; }
    public bool InLobby { get; set; }
    public bool GameOver { get; set; }
    public bool GameStarted { get; set; }
    public int StartingMoney { get; set; }
    public int ThemeId { get; set; }
    public bool FullSetDoublePropertyRent { get; set; }
    public bool ExtraMoneyForLandingOnGo { get; set; }
    public bool CollectMoneyFromFreeParking { get; set; }
    public int MoneyInFreeParking { get; set; }
    public Guid? CurrentPlayerTurn { get; set; } //Join for PlayerId on TurnOrder with GameId
    public int? ActivePlayerCount { get; set; } = 0; //Join for game PlayerId on Game when searching games
    public bool DiceRollInProgress { get; set; }
    public bool MovementInProgress { get; set; }
    //Join for LastDiceRoll properties
    public int? DiceOne { get; set; }
    public int? DiceTwo { get; set; }
    public int? UtilityDiceOne { get; set; }
    public int? UtilityDiceTwo { get; set; }
}