import { useWebSocket } from "../../../../../../../hooks/useWebSocket"
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider";

export const EndTurn = () => {
    
    const websocket = useWebSocket();
    const gameState = useGameState()

    return (
        <button
            onClick={() => {
                if(!gameState.gameState)return
                websocket.gameState.endTurn(gameState.gameState.id)
            }}
            className='bg-white p-[5px]'
        >
            End Turn
        </button>
    )
}