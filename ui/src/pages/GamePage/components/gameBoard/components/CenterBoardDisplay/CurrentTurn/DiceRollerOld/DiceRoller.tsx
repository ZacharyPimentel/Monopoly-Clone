import { Die } from "./components/Die"
import { useGameState } from "@stateProviders/GameStateProvider";

export const DiceRoller = () => {

    const gameState = useGameState();

    return (
        <div className='flex flex-col items-center gap-[50px]'>
            <div className='flex gap-[50px]'>
                <Die value={gameState.game?.utilityDiceOne || gameState.game?.diceOne || 1}/>
                <Die value={gameState.game?.utilityDiceTwo || gameState.game?.diceTwo || 1}/>
            </div>
        </div>
    )
}