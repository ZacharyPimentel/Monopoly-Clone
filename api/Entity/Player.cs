using TypeGen.Core.TypeAnnotations;
namespace api.Entity;
[ExportTsInterface]
public class Player
{
    public required Guid Id { get; set; }
    public required string PlayerName { get; set; }
    public required int IconId { get; set; }
    public bool Active { get; set; } = true;
    public int Money { get; set; }
    public int BoardSpaceId { get; set; }
    public int PreviousBoardSpaceId { get; set; }
    public bool IsReadyToPlay { get; set; } = false;
    public bool InCurrentGame { get; set; } = false;
    public int RollCount { get; set; } = 0;
    public bool CanRoll { get; set; } = true;
    public bool InJail { get; set; } = false;
    public required Guid GameId { get; set; }
    public bool RollingForUtilities { get; set; } = false;
    public int JailTurnCount { get; set; } = 0;
    public int GetOutOfJailFreeCards { get; set; } = 0;
    public bool Bankrupt { get; set; }
    public Guid? InDebtTo {get; set;}
    public int InDebtToAmount {get; set;}
    public int DebtToBank { get; set; }

    //Joined properties from PlayerIcon
    public required string IconUrl { get; set; }
    public required string IconName { get; set; }
}