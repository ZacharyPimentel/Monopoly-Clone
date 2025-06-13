
using System.ComponentModel;
namespace api.Enumerable;
public enum Errors
{
    [Description("Card does not exist.")]
    CardDoesNotExist,
    
    [Description("Card is missing it's advance to space id.")]
    CardMissingAdvanceToSpaceId,
    
    [Description("Card is missing it's payment amount.")]
    CardMissingPaymentAmount,

    [Description("Game card does not exist.")]
    GameCardDoesNotExist,

    [Description("Game does not exist.")]
    GameDoesNotExist,

    [Description("Game with this name already exists.")]
    GameNameExists,

    [Description("Game has already started.")]
    GameStarted,

    [Description("This board space doesn't have an associated property.")]
    NoBoardSpaceProperty,

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