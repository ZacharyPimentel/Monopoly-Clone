import { RollButton } from "./RollButton"
import { EndTurn } from "./EndTurn";
import { PurchaseButton } from "./PurchaseButton";
import { usePlayer } from "../../../../../../../hooks/usePlayer";
import { PayJailFee } from "./PayJailFee";
import { DiceRoller } from "./DiceRoller/DiceRoller";
import { useGameState } from "@stateProviders/GameStateProvider";
import { useEffect, useState } from "react";

export const CurrentTurn = () => {

    const {player,currentBoardSpace} = usePlayer();
    const gameState = useGameState()

    if(gameState.game?.diceRollInProgress || gameState.queueMessageCount > 0){
        return (
            <div className='flex flex-col gap-[50px]'>
                <DiceRoller/>
            </div>
        )
    }

    return (
        <div className='flex flex-col gap-[50px]'>
            <DiceRoller/>

            {/* Show purchase button if property has no player id (not owned) */}
            {
                currentBoardSpace?.property && 
                !currentBoardSpace?.property?.playerId && 
                player.rollCount > 0 && 
                
                (
                <PurchaseButton player={player} property={currentBoardSpace.property} spaceName={currentBoardSpace.boardSpaceName}/>
            )}
            {
                player.canRoll || player.rollingForUtilities
                    ?   <RollButton/>
                    :   <EndTurn/>
            }
            {player?.inJail && player.rollCount === 0 && (
                <PayJailFee/>
            )}
        </div>
    )
}