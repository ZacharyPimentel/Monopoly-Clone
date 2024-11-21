import { useGameState } from "../../../../../stateProviders/GameStateProvider"
import { useWebSocket } from "../../../../../hooks/useWebSocket";

export const Rules = () => {

    const gameState = useGameState();
    const {invoke} = useWebSocket();
    const startingMoneyOptions = [500,1000,1500,2000,2500,3000]

    return(
        <div className='flex flex-col gap-[10px] p-[30px] bg-totorogreen w-full flex-1'>
            Game Rules and Settings
            <ul>
                <li className='flex gap-[20px]'>
                    <p>Starting Money:</p>
                    <select 
                        disabled={gameState.currentSocketPlayer?.playerId && !gameState.game?.gameStarted ? false : true} 
                        value={gameState.game?.startingMoney} 
                        onChange={(e) => invoke.game.update(gameState.gameId,{startingMoney:parseInt(e.target.value)})}
                    >
                        {startingMoneyOptions.map(option => {
                            return <option key={option} value={option}>{option}</option>
                        })}
                    </select>
                </li>
            </ul>
        </div>
    )
}