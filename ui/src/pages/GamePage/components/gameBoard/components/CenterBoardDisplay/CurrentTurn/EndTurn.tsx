import { useWebSocket } from "../../../../../../../hooks/useWebSocket"
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider";

export const EndTurn = () => {
    
    const {invoke} = useWebSocket();
    const gameState = useGameState()

    return (
        <button
            onClick={() => {
                if(!gameState.game)return
                invoke.game.endTurn();
            }}
            className='bg-white p-[5px]'
        >
            End Turn
        </button>
    )
}