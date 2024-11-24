import { usePlayer } from "../../../../../../hooks/usePlayer";
import { useGameState } from "../../../../../../stateProviders/GameStateProvider"
import { CurrentTurn } from "./CurrentTurn/CurrentTurn";
import { GameLogs } from "./GameLogs";
import { NotCurrentTurn } from "./NotCurrentTurn/NotCurrentTurn";

export const CenterBoardDisplay = () => {

    const gameState = useGameState();
    const {isCurrentTurn,player} = usePlayer();

    if(gameState.game?.inLobby){
        return (
            <div className='rounded flex flex-col justify-center items-center w-full h-full gap-[20px]'>
                <p className='text-white font-bold'>Waiting to start the game until all players are ready.</p>
                <GameLogs/>
            </div>
        )
    }

    return (
        <div className='rounded flex flex-col justify-center items-center w-full h-full gap-[20px]'>
            {player && isCurrentTurn
                ? <CurrentTurn/>
                : <NotCurrentTurn/>
            }
            <GameLogs/>
        </div>
    )
}