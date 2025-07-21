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
    PlayerGoBankrupt,
    PlayerCompletePayment,
    PlayerPurchaseProperty,
    PlayerReady,
    PlayerUpdate,
    PlayerUpdateGroup,
    PlayerRollForTurn,
    PlayerRollForUtilties,
    PropertyDowngrade,
    PropertyMortgage,
    PropertyUnmortgage,
    PropertyUpgrade,
    TradeAccept,
    TradeDecline,
    TradeList,
    TradeUpdate,
}