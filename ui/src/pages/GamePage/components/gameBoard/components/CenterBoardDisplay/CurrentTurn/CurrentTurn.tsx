import { RollButton } from "./RollButton"
import { EndTurn } from "./EndTurn";
import { PurchaseButton } from "./PurchaseButton";
import { usePlayer } from "@hooks/usePlayer";
import { PayJailFee } from "./PayJailFee";
import { useGameState } from "@stateProviders/GameStateProvider";
import { BankruptButton } from "./BankruptButton";

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
                    :   player.money >= 0
                        ?   <EndTurn/>
                        : <div className='flex flex-col items-center gap-[20px]'>
                            <p className='text-white'>You are out of money! Raise some money and pay your debt!</p>
                            <p className='text-white text-[20px]'>$600 still owed to: </p>
                            <BankruptButton/>
                          </div>
            }
            {player?.inJail && player.rollCount === 0 && (
                <PayJailFee/>
            )}
        </div>
    )
}