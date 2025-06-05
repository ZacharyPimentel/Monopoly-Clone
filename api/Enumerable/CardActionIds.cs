using TypeGen.Core.TypeAnnotations;
namespace api.Enumerable;
[ExportTsEnum]
public enum CardActionIds
{
    Null,
    PayBank,
    ReceiveFrombank,
    AdvanceToSpace,
    BackThreeSpaces,
    GoToJail,
    GetOutOfJailFree,
    PayHouseHotel,
    ReceiveFromPlayers,
    AdvanceToRailroad,
    AdvanceToUtility,
    PayPlayers
}