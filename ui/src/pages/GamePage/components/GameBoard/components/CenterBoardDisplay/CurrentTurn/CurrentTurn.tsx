import { RollButton } from "./RollButton"
import { EndTurn } from "./EndTurn";
import { PurchaseButton } from "./PurchaseButton";
import { usePlayer } from "@hooks";
import { PayJailFee } from "./PayJailFee";
import { BankruptButton } from "./BankruptButton";
import { CompletePaymentButton } from "./CompletePaymentButton";
import { useGameState } from "@stateProviders";

export const CurrentTurn = () => {

    const {player,currentBoardSpace} = usePlayer();
    const gameState = useGameState(['queueMessageCount','game'])

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
                    <div className='flex flex-col items-center'>
                        {player.money < player.debts[0].amount && (
                            <>
                                <p className='text-white text-[8px]'>You don't have enough money to make your payment(s).</p>
                                <p className='text-white text-[8px]'>
                                    You still need ${currentPlayerDebtTotal - player.money} in total
                                </p>
                            </>
                        )}
                        <div className='flex gap-[5px] md:gap-[20px]'>

                            <CompletePaymentButton player={player}/>
                            <BankruptButton/>
                        </div>
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