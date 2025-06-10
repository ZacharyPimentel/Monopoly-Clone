using System.Runtime.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace api.Enumerable;

[ExportTsEnum]
public enum WebSocketEvents
{
    BoardSpaceUpdate,
    Error,
    GameCreate,
    GameUpdate,
    GameUpdateAll,
    GameUpdateGroup,
    GameLogUpdate,
    PlayerEdit,
    PlayerEndTurn,
    PlayerReady,
    PlayerUpdate,
    PlayerUpdateGroup,
    PlayerRollForTurn,
    TradeList,
    TradeUpdate,
}