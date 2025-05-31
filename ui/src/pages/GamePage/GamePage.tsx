import { GameInformation } from "./components/GameInformation/GameInformation";
import { GameBoard } from "./components/GameBoard/GameBoard";
import { useEffect } from "react";
import { useGameDispatch, useGameState } from "../../stateProviders/GameStateProvider";
import { useWebSocket } from "../../hooks/useWebSocket";
import { SocketPlayer } from "../../types/websocket/Player";
import { Player } from "../../types/controllers/Player";
import { Game } from "../../types/controllers/Game";
import { LoadingSpinner } from "../../globalComponents/LoadingSpinner";
import { useGlobalDispatch } from "../../stateProviders/GlobalStateProvider";
import { useNavigate, useParams } from "react-router-dom";
import { PlayerCreateModal } from "./modal/PlayerCreateModal";
import { BoardSpace } from "../../types/controllers/BoardSpace";
import { GameLog } from "../../types/websocket/GameLog";
import { GameMasterMenu } from "./components/GameMasterMenu";
import { Trade } from "../../types/websocket/Trade";
import { WebSocketEvents } from "@generated/WebSocketEvents";
export const GamePage = () => {

    const gameDispatch = useGameDispatch();
    const globalDispatch = useGlobalDispatch()
    const gameState = useGameState();
    const {listen,invoke,stopListen} = useWebSocket()
    const {gameId} = useParams();
    const navigate = useNavigate();

    //updates the socket group on the server to receive game events
    useEffect( () => {

        const playerUpdateCallback = (currentSocketPlayer:SocketPlayer) => gameDispatch({currentSocketPlayer})
        const playerUpdateAllCallback = (players:Player[]) => gameDispatch({players})
        const gameUpdateCallback = (game:Game | null) => {
            if(!game){
                navigate('/lobby')
                return
            }
            gameDispatch({game, gameId:game.id})
        }
        const boardSpaceUpdateCallback = (boardSpaces:BoardSpace[]) => gameDispatch({boardSpaces})
        const logUpdateCallback = (gameLogs:GameLog[]) => gameDispatch({gameLogs})
        const tradeUpdateCallback = (trades:Trade[]) => {
            console.log(trades)
            gameDispatch({trades})
        }

        listen(WebSocketEvents.GameUpdate,gameUpdateCallback)
        listen(WebSocketEvents.PlayerUpdate,playerUpdateCallback)
        listen(WebSocketEvents.PlayerUpdateGroup,playerUpdateAllCallback)
        listen(WebSocketEvents.BoardSpaceUpdate, boardSpaceUpdateCallback)
        listen(WebSocketEvents.GameLogUpdate,logUpdateCallback);
        listen(WebSocketEvents.TradeUpdate,tradeUpdateCallback);

        invoke.game.join(gameId!);

        return () => {
            stopListen(WebSocketEvents.GameUpdate,gameUpdateCallback)
            stopListen(WebSocketEvents.PlayerUpdate,playerUpdateCallback)
            stopListen(WebSocketEvents.PlayerUpdateGroup,playerUpdateAllCallback)
            stopListen(WebSocketEvents.BoardSpaceUpdate, boardSpaceUpdateCallback)
            stopListen(WebSocketEvents.GameLogUpdate,logUpdateCallback);
            stopListen(WebSocketEvents.TradeUpdate,tradeUpdateCallback);
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