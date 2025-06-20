import { usePlayer } from "@hooks/usePlayer";
import { useGameState } from "@stateProviders/GameStateProvider"
import { CurrentTurn } from "./CurrentTurn/CurrentTurn";
import { DiceRoller } from "./CurrentTurn/DiceRoller/DiceRoller";
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
            <div className='flex-1 flex justify-center items-center w-full'>
                <DiceRoller/>
            </div>
            <div className='flex-1 flex items-center w-full justify-center'>
                {player && isCurrentTurn
                    ? <CurrentTurn/>
                    : <NotCurrentTurn/>
                }
            </div>
            <div className='flex-1 flex items-center w-full'>
                <GameLogs/>
            </div>
        </div>
    )
}