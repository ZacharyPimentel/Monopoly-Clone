import { usePlayer } from "@hooks/usePlayer";
import { useWebSocket } from "@hooks/useWebSocket";
import { useGameDispatch, useGameState } from "@stateProviders/GameStateProvider"
import { useEffect, useState } from "react";

export const RollButton = () => {

    const gameState = useGameState();
    const gameDispatch = useGameDispatch()
    const {player} = usePlayer();
    const [rollInProgress,setRollInProgress] = useState(false);
    const {invoke} = useWebSocket();

    useEffect( () => {
        if(gameState?.game?.diceRollInProgress) return
        setRollInProgress(false);
    },[gameState?.game?.diceRollInProgress])

    return (
        <button
            disabled={rollInProgress || player.rollCount === 3}
            onClick={() => {
                setRollInProgress(true);
                if(player.rollingForUtilities){
                    invoke.player.rollForUtilities();
                }else{
                    invoke.player.rollForTurn();
                }
            }}
            className='bg-white p-[5px] disabled:opacity-[0.6]'
        >
            {gameState.game?.utilityDiceOne && gameState.game?.utilityDiceTwo 
                ? "Continue Turn"
                : "Roll"
            }
        </button>
    )
}