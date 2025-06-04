namespace api.DTO.Entity;

public class GameUpdateParams
{
    public bool? InLobby { get; set; }
    public bool? GameOver { get; set; }
    public bool? GameStarted { get; set; }
    public int? StartingMoney { get; set; }
    public bool? FullSetDoublePropertyRent { get; set; }
    public bool? DiceRollInProgress { get; set; }
}