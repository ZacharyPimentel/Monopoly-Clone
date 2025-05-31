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
    PlayerUpdate,
    PlayerUpdateGroup,
    TradeList,
    TradeUpdate,
}