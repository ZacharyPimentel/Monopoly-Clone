using System.Runtime.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace api.Enumerable;

[ExportTsEnum]
public enum WebSocketEvents
{
    BoardSpaceUpdate,
    Error,
    GameCreate,
    GameRulesUpdate,
    GameUpdate,
    GameUpdateAll,
    GameUpdateGroup,
    GameLogUpdate,
    PayOutOfJail,
    PlayerEdit,
    PlayerEndTurn,
    PlayerPurchaseProperty,
    PlayerReady,
    PlayerUpdate,
    PlayerUpdateGroup,
    PlayerRollForTurn,
    PlayerRollForUtilties,
    TradeList,
    TradeUpdate,
}