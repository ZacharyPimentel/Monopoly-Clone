import { useGameState } from "../../../../../stateProviders/GameStateProvider"
import { useWebSocket } from "../../../../../hooks/useWebSocket";

export const Rules = () => {

    const gameState = useGameState();
    const {invoke} = useWebSocket();
    const startingMoneyOptions = [500,1000,1500,2000,2500,3000]

    return(
        <div className='flex flex-col gap-[20px] p-[30px] bg-totorogreen w-full flex-1'>
            Game Rules and Settings
            <ul className='flex flex-col gap-[10px]'>
                {/* Starting Money */}
                <li className='flex gap-[20px]'>
                    <p className='flex-1'>Starting Money:</p>
                    <select 
                        disabled={gameState.currentSocketPlayer?.playerId && !gameState.game?.gameStarted ? false : true} 
                        value={gameState.game?.startingMoney} 
                        onChange={(e) => invoke.game.updateRules({startingMoney:parseInt(e.target.value)})}
                    >
                        {startingMoneyOptions.map(option => {
                            return <option key={option} value={option}>{option}</option>
                        })}
                    </select>
                </li>
                {/* Double base rent when full set is owned */}
                <li className='flex gap-[20px]'>
                    <p className='flex-1'>2x Rent on full set properties:</p>
                    <input 
                        checked={gameState.game?.fullSetDoublePropertyRent || false} 
                        type='checkbox' 
                        className='scale-[1.5]'
                        onChange={(e) => invoke.game.updateRules({fullSetDoublePropertyRent:e.target.checked})}
                        disabled={gameState.currentSocketPlayer?.playerId && !gameState.game?.gameStarted ? false : true} 
                    />
                </li>
            </ul>
        </div>
    )
}