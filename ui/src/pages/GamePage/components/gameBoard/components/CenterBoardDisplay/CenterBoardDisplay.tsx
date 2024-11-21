import { usePlayer } from "../../../../../../hooks/usePlayer";
import { useGameState } from "../../../../../../stateProviders/GameStateProvider"
import { CurrentTurn } from "./CurrentTurn/CurrentTurn";
import { NotCurrentTurn } from "./NotCurrentTurn/NotCurrentTurn";

export const CenterBoardDisplay = () => {

    const gameState = useGameState();
    const {isCurrentTurn,player} = usePlayer();

    if(gameState.game?.inLobby){
        return (
            <div className='rounded flex justify-center items-center w-full h-full'>
                <p className='text-white font-bold'>Waiting to start the game until all players are ready.</p>
            </div>
        )
    }

    return (
        <div className='rounded flex justify-center items-center w-full h-full'>
            {player && isCurrentTurn
                ? <CurrentTurn/>
                : <NotCurrentTurn/>
            }
        </div>
    )
}