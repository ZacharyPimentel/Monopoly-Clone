
using System.ComponentModel;
namespace api.Enumerable;
public enum WebSocketErrors
{
    [Description("Game does not exist.")]
    GameDoesNotExist,

    [Description("It is not this player's turn.")]
    NotPlayerTurn,

    [Description("Player's current boardspace doesn't match what was expected.")]
    PlayerBoardSpaceMismatch,

    [Description("Player does not exist.")]
    PlayerDoesNotExist,

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