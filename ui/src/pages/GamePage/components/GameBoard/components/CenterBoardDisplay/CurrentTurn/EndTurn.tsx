import { useWebSocket } from "@hooks"
import { useGameState } from "@stateProviders";

export const EndTurn = () => {
    
    const {invoke} = useWebSocket();
    const gameState = useGameState(['game'])

    return (
        <button
            onClick={() => {
                if(!gameState.game)return
                invoke.game.endTurn();
            }}
            className='game-button'
        >
            End Turn
        </button>
    )
}