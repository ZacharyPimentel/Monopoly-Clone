import { useMemo } from "react";
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider"
import { DiceRoller } from "./DiceRoller/DiceRoller"
import { RollButton } from "./RollButton"
import { EndTurn } from "./EndTurn";
import { PurchaseButton } from "./PurchaseButton";
import { usePlayer } from "../../../../../../../hooks/usePlayer";
import { useLandedOnSpace } from "../../../../../../../hooks/useLandedOnSpace";
import { PayJailFee } from "./PayJailFee";

export const CurrentTurn = () => {

    const gameState = useGameState();
    const {player,currentBoardSpace} = usePlayer();

    useLandedOnSpace();

    const allowedToRoll = useMemo( () => {
        if(!player)return false
        let allowed = true;
        //if player has rolled doubles 3 times, go to jail and no more rolling
        if(player.rollCount > 3){
            allowed = false;
        }
        //if player has not exceeded 3 rolls, but didn't roll doubles, not allowed to roll more
        if(player.rollCount < 3 && player.rollCount !== 0 && (gameState.game?.diceOne !== gameState.game?.diceTwo)){
            allowed = false
        }
        return allowed
    },[player])
    
    return (
        <div className='flex flex-col gap-[50px]'>
            <DiceRoller/>
            {/* Show purchase button if property has no player id (not owned) */}
            {currentBoardSpace?.property && !currentBoardSpace?.property?.playerId && player.rollCount > 0 && (
                <PurchaseButton player={player} property={currentBoardSpace.property}/>
            )}
            {
                allowedToRoll
                    ?   <RollButton/>
                    :   <EndTurn/>
            }
            {player?.inJail && player.rollCount === 0 && (
                <PayJailFee/>
            )}
        </div>
    )
}