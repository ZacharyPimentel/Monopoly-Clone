import { useEffect } from "react"
import { Die } from "./components/Die"
import { useGameDispatch, useGameState } from "../../../../../../../../stateProviders/GameStateProvider";
import { useWebSocket } from "../../../../../../../../hooks/useWebSocket";
import { usePlayer } from "../../../../../../../../hooks/usePlayer";
import { BoardSpaceCategory } from "../../../../../../../../types/enums/BoardSpaceCategory";
import { useGameMasterDispatch, useGameMasterState } from "../../../../../../../../stateProviders/GameMasterStateProvider";

export const DiceRoller:React.FC<{uiOnly?:boolean}> = ({uiOnly = false}) => {

    const gameState = useGameState();
    const gameDispatch = useGameDispatch();
    const {invoke} = useWebSocket();
    const {player,currentBoardSpace} = usePlayer();
    const {forceLandedSpace} = useGameMasterState()
    const gameMasterDispatch = useGameMasterDispatch()
    const gameMasterState = useGameMasterState();

    useEffect( () => {
        if(!gameState.rolling || uiOnly)return
        var diceOne  = gameMasterState.forceDiceOne || Math.floor((Math.random() * 6) + 1);
        var diceTwo  = gameMasterState.forceDiceTwo || Math.floor((Math.random() * 6) + 1);

        //make sure the correct dice roll is set (real roll vs utilities)
        if(player.rollingForUtilities){
            invoke.lastDiceRoll.updateUtilityDiceRoll(gameState.gameId,diceOne,diceTwo)
        }else{
            invoke.lastDiceRoll.update(gameState.gameId,diceOne,diceTwo);
            if(gameState.game?.utilityDiceOne && gameState.game?.utilityDiceTwo){
              invoke.lastDiceRoll.updateUtilityDiceRoll(gameState.gameId)
            }
        }

        setTimeout( () => {
            //handle roll logic different if player is in jail
            if(player.inJail){
                //escape jail
                if(diceOne === diceTwo){
                    invoke.gameLog.create(gameState.gameId,`${player.playerName} is free from jail.`)
                }else{
                    //logic for rolling out of jail
                    const newJailTurnCount = player.jailTurnCount + 1;
                    if(newJailTurnCount === 3){
                        invoke.player.update(player.id,{
                            inJail:false,
                            rollCount:1,
                            turnComplete:true
                        })
                        invoke.gameLog.create(gameState.gameId,`${player.playerName} is free from jail.`)
                    }else{
                        const newJailTurnCount = player.jailTurnCount + 1;
                        invoke.player.update(player.id,{turnComplete:true,rollCount:1,jailTurnCount:newJailTurnCount})
                        invoke.gameLog.create(gameState.gameId,`${player.playerName} is still in jail (${newJailTurnCount}/3)`)
                    }
                    gameDispatch({rolling:false})
                    return
                }
            }

            if(player.rollingForUtilities){
                const ownerUtilities = gameState.boardSpaces.filter( (space) => 
                    space.boardSpaceCategoryId === BoardSpaceCategory.Utility &&
                    space.property?.playerId === currentBoardSpace.property?.playerId
                );
                let amountToPay = 0
                if(ownerUtilities.length === 1){
                    amountToPay = (diceOne + diceTwo) * 4;
                }else if(ownerUtilities.length === 2){
                    amountToPay = (diceOne + diceTwo) * 10;
                }
                invoke.player.update(player.id,{
                    turnComplete:true,
                    money: player.money - amountToPay,
                    rollingForUtilities:false
                })
                const propertyOwner = gameState.players.find( (player) => player.id === currentBoardSpace.property?.playerId)!
                invoke.player.update(propertyOwner.id,{money:propertyOwner.money + amountToPay})
                invoke.gameLog.create(gameState.gameId,`${player.playerName} paid ${propertyOwner.playerName} $${amountToPay}.`)
                gameDispatch({rolling:false})
                return
            }

            //if doubles was rolled 3 times, go straight to jail
            if(player.rollCount + 1 === 3 && diceOne === diceTwo){
                invoke.player.update(player.id,{
                    inJail:true,
                    boardSpaceId: 11, // 11 is the space for jail
                    rollCount: player.rollCount + 1,
                    turnComplete:true
                })
                invoke.gameLog.create(gameState.gameId,`${player.playerName} went to jail.`)
                gameDispatch({rolling:false})
                return
            }

            //move normally otherwise
            let newBoardPosition = player.boardSpaceId + diceOne + diceTwo;
            let passedGo = false;
            //handle setting correct position when going over GO
            if(newBoardPosition > 39) {
                newBoardPosition = newBoardPosition % 40
                if (newBoardPosition > 0) passedGo = true;
            }
            if(newBoardPosition === 0) newBoardPosition = 1;

            //handle admin space override
            if(forceLandedSpace){
                newBoardPosition = forceLandedSpace
                passedGo = newBoardPosition < player.boardSpaceId
            }

            //update player
            invoke.player.update(player.id,{
                boardSpaceId: newBoardPosition,
                rollCount: player.rollCount + 1,
                inJail:false,
                //add GO money if passed
                ...(passedGo && {money:player.money + 200}),
            })
            if(passedGo){
                invoke.gameLog.create(gameState.gameId,`${player.playerName} made $200 for passing go.`)
            }
            gameDispatch({rolling:false})
            gameMasterDispatch({forceLandedSpace:0})
        },1000)
    },[gameState.rolling,gameState.currentSocketPlayer,gameState.players,player])

    return (
        <div className='flex flex-col items-center gap-[50px]'>
            <div className='flex gap-[50px]'>
                <Die value={gameState.game?.utilityDiceOne || gameState.game?.diceOne || 1}/>
                <Die value={gameState.game?.utilityDiceTwo || gameState.game?.diceTwo || 1}/>
            </div>
        </div>
    )
}