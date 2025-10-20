import { useGameState } from "@stateProviders";

export const usePlayer = () => {
    const gameState = useGameState(['players','game','boardSpaces','currentSocketPlayer']);

    const player = gameState.players.find((player) => player.id === gameState.currentSocketPlayer?.playerId)!
    const isCurrentTurn = gameState.game?.currentPlayerTurn === player?.id
    const currentBoardSpace = gameState.boardSpaces.find( (space) => space.id === player?.boardSpaceId)!
    
    return {
        player,
        isCurrentTurn,
        currentBoardSpace
    }
}