import { useEffect, useState } from "react"
import { Die } from "./components/Die"
import { useWebSocket } from "../../../../../../hooks/useWebSocket"
import { useGameState } from "../../../../../../stateProviders/GameStateProvider"

export const DiceRoller = () => {

    const gameState = useGameState();
    const [rolling,setRolling] = useState(false)
    const webSocket = useWebSocket();

    useEffect( () => {
        if(!rolling)return
        var diceOne   = Math.floor((Math.random() * 6) + 1);
        var diceTwo   = Math.floor((Math.random() * 6) + 1);
        webSocket.gameState.setLastDiceRoll([diceOne,diceTwo]);
        setRolling(false)
    },[rolling])

    return (
        <div className='flex flex-col items-center gap-[50px]'>
            <div className='flex gap-[50px]'>
                <Die value={gameState.lastDiceRoll?.[0] || 1}/>
                <Die value={gameState.lastDiceRoll?.[1] || 1}/>
            </div>
            <button disabled={false} onClick={() => setRolling(true)} className='p-[10px] bg-totorogreen w-[100px] font-bold enabled-hover:opacity-[0.8]'>Roll</button>
        </div>
        
    )
}