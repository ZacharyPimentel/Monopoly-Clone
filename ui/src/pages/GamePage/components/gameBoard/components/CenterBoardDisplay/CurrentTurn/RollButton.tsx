import { usePlayer } from "@hooks/usePlayer";
import { useGameDispatch, useGameState } from "@stateProviders/GameStateProvider"

export const RollButton = () => {

    const gameState = useGameState();
    const gameDispatch = useGameDispatch()
    const {player} = usePlayer();

    return (
        <button
            disabled={gameState.rolling || player.rollCount === 3}
            onClick={() => gameDispatch({rolling:true})}
            className='bg-white p-[5px]'
        >
            {gameState.game?.utilityDiceOne && gameState.game?.utilityDiceTwo 
                ? "Continue Turn"
                : "Roll"
            }
        </button>
    )
}