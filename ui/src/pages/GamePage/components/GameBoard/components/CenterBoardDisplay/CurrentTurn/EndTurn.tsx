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
            className='bg-white p-[5px] min-w-[50px] md:min-w-[100px] text-xs md:text-lg rounded'
        >
            End Turn
        </button>
    )
}