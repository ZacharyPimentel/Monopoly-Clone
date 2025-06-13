import { TradeCreateParams } from "@generated/TradeCreateParams";
import { useGameState } from "../stateProviders/GameStateProvider"
import { useGlobalState } from "../stateProviders/GlobalStateProvider";
import { Game } from "../types/controllers/Game";
import { PropertyUpdateParams } from "../types/controllers/Property";
import { GameUpdateParams } from "../types/websocket/Game";
import { SocketEventPlayerEdit, SocketEventPlayerReady, SocketEventPlayerUpdate, SocketEventPurchaseProperty, SocketEventRulesUpdate, SocketEventTradeUpdate, WebSocketEvents } from "@generated/index";
import { getEnumNameFromValue } from "src/helpers/getEnumNameFromValue";
import { useCallback, useEffect, useRef, useState } from "react";

type Queue = {
    [key:number]:{
        processing:boolean,
        callbackFunctions:Function[]
    }
}

export const useWebSocket = () => {
    const globalState = useGlobalState();
    const gameState = useGameState();

    const queuedCallbacks = useRef<Queue>({});
    const socketHandlers = useRef<Map<number, (data: any) => void>>(new Map());

    const processQueueMessage = useCallback((eventEnum:number) => {
        if(!queuedCallbacks.current)return
        
        const queue = queuedCallbacks.current[eventEnum]
        if(queue.processing)return
        const nextCallback = queue.callbackFunctions.shift();
        if(nextCallback){
            queue.processing = true;
            nextCallback();
            setTimeout( () => {
                queue.processing = false;
                processQueueMessage(eventEnum)
            },500)
        }
    },[queuedCallbacks])

    const handleSocketEvent = useCallback ((eventEnum: number, callback: (data: any) => void) => {
        const queueHandler = (data:any) => {
            if (!queuedCallbacks.current[eventEnum]) {
                queuedCallbacks.current[eventEnum] = {
                    processing: false,
                    callbackFunctions: []
                };
            }
            //add socket event callback to the queue
            queuedCallbacks.current && queuedCallbacks.current[eventEnum].callbackFunctions.push(() => callback(data))
            //process the queue
            processQueueMessage(eventEnum);
        }
        // Save the wrapped handler so we can unregister it later
        socketHandlers.current.set(eventEnum, queueHandler);

        return queueHandler;
    },[]);

    return {
        invoke:{
            //game events not tied to a table
            gameState:{
                setLastDiceRoll: (rolls:number[]) => {
                    globalState.ws.invoke("SetLastDiceRoll",rolls)
                },
                updateRules: (rule:Partial<Game>) => {
                    globalState.ws.invoke("UpdateRules",gameState.game?.id,rule)
                },
                endTurn: (gameId:number) => {
                    globalState.ws.invoke("endTurn",gameId);
                }
            },
            game:{
                getAll: () => {
                    globalState.ws.invoke('GameGetAll');
                },
                create: (gameName:string,themeId:number) => {
                    globalState.ws.invoke('GameCreate',{gameName,themeId});
                },
                join: (gameId:string) => {
                    globalState.ws.invoke('GameJoin',gameId)
                },
                leave: (gameId:string) => {
                    globalState.ws.invoke('GameLeave',gameId);
                },
                updateRules: (rulesUpdateParams:SocketEventRulesUpdate) => {
                    globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.GameRulesUpdate),rulesUpdateParams);
                },
                endTurn: () => {
                    globalState.ws.invoke('GameEndTurn');
                }
            },
            gameLog:{
                create: (gameId:string,message:string) => {
                    globalState.ws.invoke('GameLogCreate',gameId,message)
                }
            },
            lastDiceRoll:{
                update: (gameId:string,diceOne:number,diceTwo:number) => {
                    globalState.ws.invoke('LastDiceRollUpdate',gameId,diceOne,diceTwo)
                },
                updateUtilityDiceRoll: (gameId:string,diceOne?:number,diceTwo?:number) => {
                    globalState.ws.invoke("LastUtilityDiceRollUpdate",gameId,diceOne,diceTwo)
                },
            },
            player:{
                create:(playerName:string,iconId:number,gameId:string) => {
                    globalState.ws.invoke('PlayerCreate',{playerName,iconId,gameId})
                },
                edit: (playerEditParams:SocketEventPlayerEdit) => {
                    globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerEdit),playerEditParams);
                },
                endTurn:() => {
                    globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerEndTurn))
                },
                getAll: () => {
                    globalState.ws.invoke("PlayerGetAll")
                },
                purchaseProperty: (purchaseParams:SocketEventPurchaseProperty) => {
                    globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerPurchaseProperty),purchaseParams)
                },
                reconnect: (playerId:string) => {
                    globalState.ws.invoke('PlayerReconnect',playerId)
                },
                rollForTurn: () => {
                    globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerRollForTurn))
                },
                setReadyStatus: (playerReadyParams:SocketEventPlayerReady) => {
                    globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerReady),playerReadyParams);
                },
                update: (updateParams:SocketEventPlayerUpdate) => {
                    globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerUpdate),updateParams)
                },
            },
            gameProperty:{
                update:(gamePropertyId:number,updateParams:Partial<PropertyUpdateParams>) => {
                    globalState.ws.invoke("GamePropertyUpdate",gamePropertyId,updateParams)
                }
            },
            trade:{
                create:(tradeCreateParams:TradeCreateParams) => {
                    globalState.ws.invoke("TradeCreate",tradeCreateParams)
                },
                update:(TradeUpdateParams:SocketEventTradeUpdate) => {
                    globalState.ws.invoke("TradeUpdate",TradeUpdateParams)
                }
            }
        },
        listen: (eventEnum:number, callback:(data:any) => void) => {
            
            const queueFunction = handleSocketEvent(eventEnum,callback)

            globalState.ws.on(eventEnum.toString(), queueFunction)
                
        },
        stopListen: (eventEnum:number) => {
            const handler = socketHandlers.current.get(eventEnum);
            if(handler){
                globalState.ws.off(eventEnum.toString(),handler)
            }
            socketHandlers.current.delete(eventEnum); // Clean up
        },
    }
}