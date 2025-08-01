import { GameInformation } from "./components/GameInformation/GameInformation";
import { GameBoard } from "./components/GameBoard/GameBoard";
import { useEffect } from "react";
import { useGameState } from "../../stateProviders/GameStateProvider";
import { useWebSocket } from "../../hooks/useWebSocket";
import { LoadingSpinner } from "../../globalComponents/LoadingSpinner";
import { useGlobalDispatch } from "../../stateProviders/GlobalStateProvider";
import { useParams } from "react-router-dom";
import { PlayerCreateModal } from "../../globalComponents/GlobalModal/modalContent/PlayerCreateModal";
import { WebSocketEvents } from "@generated/WebSocketEvents";
import { GameOverModal } from "@globalComponents/GlobalModal/modalContent/GameOverModal";

export const GamePage = () => {

    const globalDispatch = useGlobalDispatch()
    const gameState = useGameState();
    const {listen,invoke,stopListen} = useWebSocket()
    const {gameId} = useParams();

    //updates the socket group on the server to receive game events
    useEffect( () => {
        listen(WebSocketEvents.PlayerUpdate)
        listen(WebSocketEvents.Error)
        listen(WebSocketEvents.GameStateUpdate);
        invoke.game.join(gameId!);
        return () => {
            stopListen(WebSocketEvents.PlayerUpdate)
            stopListen(WebSocketEvents.Error)
            stopListen(WebSocketEvents.GameStateUpdate);
            invoke.game.leave(gameId!);
        }
    },[])

    useEffect( () => {
        if(!gameState.currentSocketPlayer || !gameState.game) return
        if(!gameState.currentSocketPlayer.playerId){
          globalDispatch({modalOpen:true,modalContent:<PlayerCreateModal/>})
        }
      },[gameState.currentSocketPlayer])

    useEffect( () => {
        if(gameState.game?.gameOver){
            globalDispatch({modalOpen:true,modalContent:<GameOverModal/>})
        }
    },[gameState.game])

    //only let people through once they've connected to the socket
    if(!gameState || !gameState.currentSocketPlayer || gameState.boardSpaces.length === 0){
        return <div className='flex justify-center items-center h-full w-full'><LoadingSpinner/></div>
    }

    return (
        <div className='flex flex-col relative'>
            <div className='justify-center w-full h-full flex flex-wrap'>
                <GameBoard/>
                <div className='h-[100vh] min-w-[300px] flex-1 relative overflow-y-scroll'>
                    <GameInformation />
                </div>
            </div>
        </div>
    )
}