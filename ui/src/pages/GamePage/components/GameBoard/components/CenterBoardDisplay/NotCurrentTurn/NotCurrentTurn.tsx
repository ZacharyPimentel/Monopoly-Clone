import { useGameState } from "@stateProviders";

export const NotCurrentTurn = () => {

    const gameState = useGameState(['players','game']);
    const currentTurnPlayer = gameState.players.find( (player) => player.id === gameState.game?.currentPlayerTurn)

    return (
        <div className='flex flex-col gap-[50px]'>
            <div className='flex gap-[20px] justify-center items-center'>
                <img className='w-[50px]' src={currentTurnPlayer?.iconUrl}/>
                <p className='text-white'>{currentTurnPlayer?.playerName} is playing</p>
            </div>
        </div>
    )
}