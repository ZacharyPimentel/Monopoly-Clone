
using System.ComponentModel;
namespace api.Enumerable;
public enum WebSocketErrors
{
    [Description("Game does not exist.")]
    GameDoesNotExist,

    [Description("Game with this name already exists.")]
    GameNameExists,

    [Description("Game has already started.")]
    GameStarted,

    [Description("It is not this player's turn.")]
    NotPlayerTurn,

    [Description("This player is currently active.")]
    PlayerActive,

    [Description("Player can continue rolling.")]
    PlayerAllowedToRoll,

    [Description("Player's current boardspace doesn't match what was expected.")]
    PlayerBoardSpaceMismatch,

    [Description("Player does not exist.")]
    PlayerDoesNotExist,

    [Description("This player is not currently active.")]
    PlayerInactive,
    
    [Description("Player is not able to continue rolling.")]
    PlayerNotAllowedToRoll,

    [Description("Property is already owned by another player.")]
    PropertyAlreadyOwned,

    [Description("Socket Connection already has a player id.")]
    SocketConnectionHasPlayerId,

    [Description("Socket Connection already has a game id.")]
    SocketConnectionHasGameId,

    [Description("Socket Connection does not have a game id.")]
    SocketConnectionMissingGameId,

    [Description("Socket Connection does not have a player id.")]
    SocketConnectionMissingPlayerId,
}