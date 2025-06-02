using System.Runtime.Serialization;
using TypeGen.Core.TypeAnnotations;

namespace api.Enumerable;

[ExportTsEnum]
public enum WebSocketEvents
{
    BoardSpaceUpdate,
    GameCreate,
    GameUpdate,
    GameUpdateAll,
    GameUpdateGroup,
    GameLogUpdate,
    PlayerEdit,
    PlayerReady,
    PlayerUpdate,
    PlayerUpdateGroup,
    TradeList,
    TradeUpdate,
}