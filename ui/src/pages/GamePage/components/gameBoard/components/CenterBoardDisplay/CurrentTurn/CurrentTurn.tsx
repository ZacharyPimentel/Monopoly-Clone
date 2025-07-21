import { RollButton } from "./RollButton"
import { EndTurn } from "./EndTurn";
import { PurchaseButton } from "./PurchaseButton";
import { usePlayer } from "@hooks/usePlayer";
import { PayJailFee } from "./PayJailFee";
import { useGameState } from "@stateProviders/GameStateProvider";
import { BankruptButton } from "./BankruptButton";
import { CompletePaymentButton } from "./CompletePaymentButton";

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
            {player.money < 0 || player.moneyNeededForPayment
                ? 
                    <div className='flex flex-col items-center gap-[20px]'>
                        {player.money < player.moneyNeededForPayment && (
                            <>
                                <p className='text-white'>You don't have enough money to make your payment.</p>
                                <p className='text-white text-[20px]'>
                                    You still need ${player.moneyNeededForPayment - player.money}.
                                </p>
                            </>
                        )}
                        <CompletePaymentButton player={player}/>
                        <BankruptButton/>
                    </div>
                : player.canRoll || player.rollingForUtilities
                    ? <RollButton/>
                    : <EndTurn/>
            }
            {player?.inJail && player.rollCount === 0 && player.money >= 50 && (
                <PayJailFee/>
            )}
        </div>
    )
}