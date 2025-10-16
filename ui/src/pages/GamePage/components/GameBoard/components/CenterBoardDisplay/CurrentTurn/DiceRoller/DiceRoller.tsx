import { useGameState } from "@stateProviders/GameStateProvider";
import { Die } from "./Die";

export const DiceRoller = () => {

    const gameState = useGameState();

    return (
        <div className='flex gap-[50px]'>
            <Die dieNumber={gameState.game?.utilityDiceOne || gameState.game?.diceOne || 1}/>
            <Die dieNumber={gameState.game?.utilityDiceTwo || gameState.game?.diceTwo || 1}/>
        </div>
    )
}