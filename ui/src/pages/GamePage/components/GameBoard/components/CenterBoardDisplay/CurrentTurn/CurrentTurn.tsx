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

    const currentPlayerDebtTotal = player.debts.reduce( (sum,debt) => sum + debt.amount, 0)
    
    return (
        <div className='flex gap-[50px] flex-wrap justify-center'>
            {/* Show purchase button if property has no player id (not owned) */}
            {
                currentBoardSpace?.property && 
                !currentBoardSpace?.property?.playerId && 
                player.rollCount > 0 && 
                
                (
                <PurchaseButton player={player} property={currentBoardSpace.property} spaceName={currentBoardSpace.boardSpaceName}/>
            )}
            {player.money < 0 || player.debts.length > 0
                ? 
                    <div className='flex flex-col items-center gap-[20px]'>
                        {player.money < player.debts[0].amount && (
                            <>
                                <p className='text-white'>You don't have enough money to make your payment(s).</p>
                                <p className='text-white text-[20px]'>
                                    You still need ${currentPlayerDebtTotal - player.money} in total
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
            {player?.inJail && player.rollCount === 0 && (
                <PayJailFee player={player}/>
            )}
        </div>
    )
}