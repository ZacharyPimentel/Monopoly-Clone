import { Game, GameStateResponse, WebSocketEvents,SocketPlayer } from "@generated";
import { MutableRefObject, useCallback, useMemo, useRef } from "react"
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";
import { useGlobalState,  useGameState } from "@stateProviders";

type Queue = {
    processingQueue:boolean
    queue: Function[]
}

export const useWebSocketCallback = () => {

    const {dispatch:gameDispatch} = useGameState([]);
    const {dispatch:globalDispatch} = useGlobalState([]);
    const navigate = useNavigate();

    const queueRef: MutableRefObject<Queue> = useRef({
        processingQueue:false,
        queue: []
    });

    const processNextCallback = useCallback(() => {
        if(!queueRef.current) return
        const nextCallback = queueRef.current.queue.shift();
        if(nextCallback){
            nextCallback();
            gameDispatch({queueMessageCount:queueRef.current.queue.length})
            setTimeout( () => {
                processNextCallback()
            },500)
        }else{
            queueRef.current.processingQueue = false;
        }
    },[]);

    const socketEventCallbacks = useMemo( () => {

        return {
            [WebSocketEvents.PlayerUpdate] : (currentSocketPlayer:SocketPlayer) => {
                gameDispatch({currentSocketPlayer})
            },
            [WebSocketEvents.GameCreate] : (gameId:string) => {
                navigate(`/game/${gameId}`)
            },
            [WebSocketEvents.GameUpdateAll] : (games:Game[]) => {
                globalDispatch({availableGames:games})
            },
            [WebSocketEvents.Error] : (message:string) => {
                toast(message,{type:'error'})
                navigate('/lobby')
            },
            [WebSocketEvents.GameStateUpdate] : (gameData:GameStateResponse) => {
                if(gameData.game !== null && !gameData.game){
                    navigate('/lobby')
                    return
                }
                //logic to determine if messages should be queued or not

                if(gameData.game){
                    if( gameData.game.diceRollInProgress || 
                        gameData.game.movementInProgress || 
                        queueRef.current.processingQueue
                    ){
                        queueRef.current.queue.push( () => {
                            gameDispatch({...gameData})
                        })
                        if (!queueRef.current.processingQueue) {
                            queueRef.current.processingQueue = true;
                            processNextCallback();
                        }
                    }else{
                        gameDispatch({...gameData});
                    }
                }else{
                    if(gameData.game == null){
                        delete gameData.game;
                    }
                    gameDispatch({...gameData});
                }
            },
            [WebSocketEvents.PasswordValidated]:({gameId,valid}:{gameId:string,valid:boolean}) => {
                if(valid){
                    navigate(`/game/${gameId}`)
                }
            }
        }
    },[])

    return socketEventCallbacks;
}