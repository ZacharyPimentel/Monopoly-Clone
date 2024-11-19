import { useEffect } from "react"
import { Die } from "./components/Die"
import { useGameDispatch, useGameState } from "../../../../../../../../stateProviders/GameStateProvider";
import { useWebSocket } from "../../../../../../../../hooks/useWebSocket";
import { usePlayer } from "../../../../../../../../hooks/usePlayer";

export const DiceRoller:React.FC<{uiOnly?:boolean}> = ({uiOnly = false}) => {

    const gameState = useGameState();
    const gameDispatch = useGameDispatch();
    const {invoke} = useWebSocket();
    const {player} = usePlayer();

    useEffect( () => {
        if(!gameState.rolling || uiOnly)return
        var diceOne  = Math.floor((Math.random() * 6) + 1);
        var diceTwo  = Math.floor((Math.random() * 6) + 1);

        invoke.lastDiceRoll.update(gameState.gameId,diceOne,diceTwo);
        setTimeout( () => {
            //handle roll logic different if player is in jail
            if(player.inJail){
                if(diceOne === diceTwo){
                    invoke.player.update(player.id,{inJail:false,turnComplete:true})
                }
                return
            }

            let newBoardPosition = player.boardSpaceId + diceOne + diceTwo;
            let passedGo = false;
            //handle setting correct position when going over GO
            if(newBoardPosition > 39) {
                newBoardPosition = newBoardPosition % 40
                if (newBoardPosition > 0) passedGo = true;
            }
            if(newBoardPosition === 0) newBoardPosition = 1;

            //update player
            invoke.player.update(player.id,{
                boardSpaceId: newBoardPosition,
                rollCount: player.rollCount + 1,
                //add GO money if passed
                ...(passedGo && {money:player.money + 200}),
                //if 3rd roll and doubles, go to jail
                ...(player.rollCount+1 === 3 && (diceOne === diceTwo) && {inJail:true})
            })
            gameDispatch({rolling:false})
        },1000)
    },[gameState.rolling,gameState.currentSocketPlayer,gameState.players,player])

    return (
        <div className='flex flex-col items-center gap-[50px]'>
            <div className='flex gap-[50px]'>
                <Die value={gameState.gameState?.diceOne || 1}/>
                <Die value={gameState.gameState?.diceTwo || 1}/>
            </div>
        </div>
    )
}