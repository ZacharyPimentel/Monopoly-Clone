import { useGameDispatch, useGameState } from "../../../../../../../stateProviders/GameStateProvider"

export const RollButton = () => {

    const gameState = useGameState();
    const gameDispatch = useGameDispatch()

    return (
        <button
            disabled={gameState.rolling}
            onClick={() => gameDispatch({rolling:true})}
            className='bg-white p-[5px]'
        >
            Roll
        </button>
    )
}