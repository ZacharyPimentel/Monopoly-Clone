using api.Entity;
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Entity;

[ExportTsInterface]
public class PlayerUpdateParams
{
    public bool? Active { get; set; }
    public int? BoardSpaceId { get; set; }
    public int? PreviousBoardSpaceId { get; set; }
    public int? GetOutOfJailFreeCards { get; set; }
    public int? IconId { get; set; }
    public bool? InCurrentGame { get; set; }
    public bool? InJail { get; set; }
    public bool? IsReadyToPlay { get; set; }
    public int? JailTurnCount { get; set; }
    public int? Money { get; set; }
    public string? PlayerName { get; set; }
    public int? RollCount { get; set; }
    public bool? RollingForUtilities { get; set; }
    public bool? CanRoll { get; set; }
    public bool? Bankrupt { get; set; }
    public static PlayerUpdateParams FromPlayer(Player player)
    {
        return new PlayerUpdateParams
        {
            Active = player.Active,
            PreviousBoardSpaceId = player.PreviousBoardSpaceId,
            BoardSpaceId = player.BoardSpaceId,
            GetOutOfJailFreeCards = player.GetOutOfJailFreeCards,
            IconId = player.IconId,
            InCurrentGame = player.InCurrentGame,
            InJail = player.InJail,
            IsReadyToPlay = player.IsReadyToPlay,
            JailTurnCount = player.JailTurnCount,
            Money = player.Money,
            PlayerName = player.PlayerName,
            RollCount = player.RollCount,
            RollingForUtilities = player.RollingForUtilities,
            CanRoll = player.CanRoll,
            Bankrupt = player.Bankrupt,
        };
    }
}