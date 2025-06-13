import { RollButton } from "./RollButton"
import { EndTurn } from "./EndTurn";
import { PurchaseButton } from "./PurchaseButton";
import { usePlayer } from "../../../../../../../hooks/usePlayer";
import { PayJailFee } from "./PayJailFee";
import { DiceRoller } from "./DiceRoller/DiceRoller";

export const CurrentTurn = () => {

    const {player,currentBoardSpace} = usePlayer();

    return (
        <div className='flex flex-col gap-[50px]'>
            <DiceRoller/>
            {/* Show purchase button if property has no player id (not owned) */}
            {currentBoardSpace?.property && !currentBoardSpace?.property?.playerId && player.rollCount > 0 && (
                <PurchaseButton player={player} property={currentBoardSpace.property} spaceName={currentBoardSpace.boardSpaceName}/>
            )}
            {
                player.canRoll
                    ?   <RollButton/>
                    :   <EndTurn/>
            }
            {player?.inJail && player.rollCount === 0 && (
                <PayJailFee/>
            )}
        </div>
    )
}