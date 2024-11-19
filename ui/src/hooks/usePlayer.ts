import { useGameState } from "../stateProviders/GameStateProvider"

export const usePlayer = () => {
    const gameState = useGameState();

    const player = gameState.players.find((player) => player.id === gameState.currentSocketPlayer?.playerId)!
    const isCurrentTurn = gameState.gameState?.currentPlayerTurn === player?.id
    const currentBoardSpace = gameState.boardSpaces.find( (space) => space.id === player?.boardSpaceId)!
    
    return {
        player,
        isCurrentTurn,
        currentBoardSpace
    }
}