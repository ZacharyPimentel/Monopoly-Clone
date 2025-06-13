import { GameInformation } from "./components/GameInformation/GameInformation";
import { GameBoard } from "./components/GameBoard/GameBoard";
import { useEffect } from "react";
import { useGameState } from "../../stateProviders/GameStateProvider";
import { useWebSocket } from "../../hooks/useWebSocket";
import { LoadingSpinner } from "../../globalComponents/LoadingSpinner";
import { useGlobalDispatch } from "../../stateProviders/GlobalStateProvider";
import { useParams } from "react-router-dom";
import { PlayerCreateModal } from "./modal/PlayerCreateModal";
import { GameMasterMenu } from "./components/GameMasterMenu";
import { WebSocketEvents } from "@generated/WebSocketEvents";
import { useWebSocketCallback } from "@hooks/useWebSocketCallback";

export const GamePage = () => {

    const globalDispatch = useGlobalDispatch()
    const gameState = useGameState();
    const {listen,invoke,stopListen} = useWebSocket()
    const {gameId} = useParams();
    const websocketCallback = useWebSocketCallback();

    //updates the socket group on the server to receive game events
    useEffect( () => {

        listen(WebSocketEvents.GameUpdate,websocketCallback.gameUpdateCallback)
        listen(WebSocketEvents.PlayerUpdate,websocketCallback.playerUpdateCallback)
        listen(WebSocketEvents.PlayerUpdateGroup,websocketCallback.playerUpdateAllCallback)
        listen(WebSocketEvents.BoardSpaceUpdate, websocketCallback.boardSpaceUpdateCallback)
        listen(WebSocketEvents.GameLogUpdate,websocketCallback.logUpdateCallback);
        listen(WebSocketEvents.TradeUpdate,websocketCallback.tradeUpdateCallback);
        listen(WebSocketEvents.Error, websocketCallback.errorCallback)

        invoke.game.join(gameId!);


        return () => {
            stopListen(WebSocketEvents.GameUpdate)
            stopListen(WebSocketEvents.PlayerUpdate)
            stopListen(WebSocketEvents.PlayerUpdateGroup)
            stopListen(WebSocketEvents.BoardSpaceUpdate)
            stopListen(WebSocketEvents.GameLogUpdate);
            stopListen(WebSocketEvents.TradeUpdate);
            stopListen(WebSocketEvents.Error)
            invoke.game.leave(gameId!);
        }
    },[])

    useEffect( () => {
        if(!gameState.currentSocketPlayer || !gameState.game) return
        if(!gameState.currentSocketPlayer.playerId){
          globalDispatch({modalOpen:true,modalContent:<PlayerCreateModal/>})
        }
      },[gameState.currentSocketPlayer])

    //only let people through once they've connected to the socket
    if(!gameState || !gameState.currentSocketPlayer || gameState.boardSpaces.length === 0){
        return <div className='flex justify-center items-center h-full w-full'><LoadingSpinner/></div>
    }

    return (
        <div className='flex flex-col relative'>
            <GameMasterMenu/>
            <div className='justify-center w-full h-full flex flex-wrap'>
                <GameBoard/>
                <div className='h-[100vh] min-w-[300px] flex-1 relative overflow-y-scroll'>
                    <GameInformation />
                </div>
            </div>
        </div>
    )
}