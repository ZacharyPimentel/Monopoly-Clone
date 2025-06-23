using System.Runtime.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace api.Enumerable;

[ExportTsEnum]
public enum WebSocketEvents
{
    BoardSpaceUpdate,
    Error,
    GameCreate,
    GameStateUpdate,
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
    TradeDecline,
    TradeList,
    TradeUpdate,
}