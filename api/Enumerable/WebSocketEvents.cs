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
    PlayerEdit,
    PlayerEndTurn,
    PlayerPurchaseProperty,
    PlayerReady,
    PlayerUpdate,
    PlayerUpdateGroup,
    PlayerRollForTurn,
    TradeList,
    TradeUpdate,
}