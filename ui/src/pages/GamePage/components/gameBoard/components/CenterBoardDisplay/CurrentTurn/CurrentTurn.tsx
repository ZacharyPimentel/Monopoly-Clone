import { RollButton } from "./RollButton"
import { EndTurn } from "./EndTurn";
import { PurchaseButton } from "./PurchaseButton";
import { usePlayer } from "@hooks/usePlayer";
import { PayJailFee } from "./PayJailFee";
import { useGameState } from "@stateProviders/GameStateProvider";

export const CurrentTurn = () => {

    const {player,currentBoardSpace} = usePlayer();
    const gameState = useGameState()

    if(gameState.game?.diceRollInProgress || gameState.queueMessageCount > 0){
        return null
    }

    return (
        <div className='flex gap-[50px]'>
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