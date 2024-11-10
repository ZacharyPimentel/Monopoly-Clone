import { useEffect, useMemo } from "react";
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider"
import { DiceRoller } from "./DiceRoller/DiceRoller"
import { RollButton } from "./RollButton"
import { EndTurn } from "./EndTurn";
import { BoardSpaceCategory } from "../../../../../../../types/enums/BoardSpaceCategory";
import { useLandedSpaceAction } from "../../../../../../../hooks/useLandedSpaceAction";
import { PurchaseButton } from "./PurchaseButton";

export const CurrentTurn = () => {

    const gameState = useGameState();
    const currentPlayer = gameState.players.find(player => player.id === gameState.currentSocketPlayer?.playerId)
    const purchaseableProperty = useLandedSpaceAction(currentPlayer)
    console.log('14',purchaseableProperty)
    const allowedToRoll = useMemo( () => {
        if(!currentPlayer) return false
        let allowed = true;
        //if player has rolled doubles 3 times, go to jail and no more rolling
        if(currentPlayer.rollCount > 3){
            allowed = false;
        }
        if(!gameState.lastDiceRoll){
            return allowed
        }
        //if player has not exceeded 3 rolls, but didn't roll doubles, not allowed to roll more
        if(currentPlayer.rollCount < 3 && currentPlayer.rollCount !== 0 && (gameState.lastDiceRoll[0] !== gameState.lastDiceRoll[1])){
            allowed = false
        }
        return allowed
    },[currentPlayer])

    if(!currentPlayer)return null
    
    return (
        <div className='flex flex-col gap-[50px]'>
            <DiceRoller/>
            {purchaseableProperty && currentPlayer && (
                <PurchaseButton player={currentPlayer} property={purchaseableProperty}/>
            )}
            {
                allowedToRoll
                    ?   <RollButton/>
                    :   <EndTurn/>
            }
        </div>
    )
}