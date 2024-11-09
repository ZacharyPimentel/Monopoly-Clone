import { useEffect } from "react"
import { Die } from "./components/Die"
import { useGameDispatch, useGameState } from "../../../../../../../../stateProviders/GameStateProvider";
import { useWebSocket } from "../../../../../../../../hooks/useWebSocket";

export const DiceRoller:React.FC<{uiOnly?:boolean}> = ({uiOnly = false}) => {

    const gameState = useGameState();
    const gameDispatch = useGameDispatch();
    const webSocket = useWebSocket();

    useEffect( () => {
        if(!gameState.rolling || uiOnly)return
        var diceOne  = Math.floor((Math.random() * 6) + 1);
        var diceTwo  = Math.floor((Math.random() * 6) + 1);
        webSocket.gameState.setLastDiceRoll([diceOne,diceTwo]);
        setTimeout( () => {
            const currentPlayer = gameState.players.find(player => player.id === gameState.currentSocketPlayer?.playerId)
            if(currentPlayer){
                let newBoardPosition = currentPlayer.boardSpaceId + diceOne + diceTwo;
                if(newBoardPosition > 39) newBoardPosition = newBoardPosition % 40
                if(newBoardPosition === 0) newBoardPosition = 1;
                webSocket.player.update(currentPlayer.id,{
                    boardSpaceId: newBoardPosition,
                    rollCount: currentPlayer.rollCount + 1
                })
            }
            gameDispatch({rolling:false})
        },1000)
    },[gameState.rolling,gameState.currentSocketPlayer,gameState.players])

    console.log('last dice roll',gameState.lastDiceRoll)

    return (
        <div className='flex flex-col items-center gap-[50px]'>
            <div className='flex gap-[50px]'>
                <Die value={gameState.lastDiceRoll?.[0] || 1}/>
                <Die value={gameState.lastDiceRoll?.[1] || 1}/>
            </div>
        </div>
    )
}