import { GameInformation } from "./components/GameInformation/GameInformation";
import { GameBoard } from "./components/GameBoard/GameBoard";
import { useEffect } from "react";
import { useGameDispatch, useGameState } from "../../stateProviders/GameStateProvider";
import { useWebSocket } from "../../hooks/useWebSocket";
import { SocketPlayer } from "../../types/websocket/Player";
import { Player } from "../../types/controllers/Player";
import { Game } from "../../types/controllers/Game";
import { useApi } from "../../hooks/useApi";
import { LoadingSpinner } from "../../globalComponents/LoadingSpinner";
import { useGlobalDispatch } from "../../stateProviders/GlobalStateProvider";
import { PlayerCreateModal } from "../../globalComponents/GlobalModal/modalContent/PlayerCreateModal";
import { useParams } from "react-router-dom";
export const GamePage = () => {

    const gameDispatch = useGameDispatch();
    const globalDispatch = useGlobalDispatch()
    const gameState = useGameState();
    const api = useApi();
    const {listen,invoke,stopListen} = useWebSocket()
    const {gameId} = useParams();

    //updates the socket group on the server to receive game events
    useEffect( () => {

        const playerUpdateCallback = (currentSocketPlayer:SocketPlayer) => gameDispatch({currentSocketPlayer})
        const playerUpdateAllCallback = (players:Player[]) => gameDispatch({players})
        const gameUpdateCallback = (game:Game) => gameDispatch({gameState:game})

        listen('game:update',gameUpdateCallback)
        listen('player:update',playerUpdateCallback)
        listen('player:updateGroup',playerUpdateAllCallback)

        invoke.game.join(gameId!);

        api.boardSpace.getAll()
        .then(boardSpaces => {
            gameDispatch({
                boardSpaces,
                gameId
            })
        })

        return () => {
            stopListen('game:update',gameUpdateCallback)
            stopListen('player:update',playerUpdateCallback)
            stopListen('player:updateGroup',playerUpdateAllCallback)
        }
    },[])

    useEffect( () => {
        if(!gameState.currentSocketPlayer || !gameState.gameState) return
        console.log('53',gameState.currentSocketPlayer.playerId)
        if(!gameState.currentSocketPlayer.playerId){
          globalDispatch({modalOpen:true,modalContent:<PlayerCreateModal/>})
        }
      },[gameState.currentSocketPlayer])

    //only let people through once they've connected to the socket
    if(!gameState || !gameState.currentSocketPlayer || gameState.boardSpaces.length === 0){
        return <div className='flex justify-center items-center h-full w-full'><LoadingSpinner/></div>
    }

    return (
        <div className='flex flex-col'>
            <div className='justify-center w-full h-full flex flex-wrap'>
                <GameBoard/>
                <div className='h-[100vh] min-w-[300px] flex-1 relative overflow-y-scroll'>
                    <GameInformation />
                </div>
            </div>
        </div>
    )
}