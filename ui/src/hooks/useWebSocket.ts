import { TradeCreateParams } from "@generated/TradeCreateParams";
import { useGameState } from "../stateProviders/GameStateProvider"
import { useGlobalState } from "../stateProviders/GlobalStateProvider";
import { Game } from "../types/controllers/Game";
import { PropertyUpdateParams } from "../types/controllers/Property";
import {  SocketEventGameCreate, SocketEventPasswordValidate, SocketEventPlayerEdit, SocketEventPlayerReady, SocketEventPlayerUpdate, SocketEventPurchaseProperty, SocketEventRulesUpdate, SocketEventTradeAccept, SocketEventTradeDecline, SocketEventTradeUpdate, WebSocketEvents } from "@generated/index";
import { getEnumNameFromValue } from "src/helpers/getEnumNameFromValue";
import { useCallback, useMemo, useRef } from "react";
import { useWebSocketCallback } from "./useWebSocketCallback";

type Queue = {
    [key:number]:{
        processing:boolean,
        callbackFunctions:Function[]
    }
}

export const useWebSocket = () => {
    const globalState = useGlobalState();
    const gameState = useGameState();
    const webSocketCallbacks = useWebSocketCallback();
    const queuedCallbacks = useRef<Queue>({});
    
    const processQueueMessage = useCallback((eventEnum:number) => {
        if(!queuedCallbacks.current)return
        
        const queue = queuedCallbacks.current[eventEnum]
        if(queue.processing)return
        const nextCallback = queue.callbackFunctions.shift();
        if(nextCallback){
            queue.processing = true;
            nextCallback();
            const timeoutLength = 0;
            setTimeout( () => {
                queue.processing = false;
                processQueueMessage(eventEnum)
            },timeoutLength)
        }
    },[queuedCallbacks])

    const webSocketCalls = useMemo( () => {
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
                    create: (createParams:SocketEventGameCreate) => {
                        globalState.ws.invoke('GameCreate',createParams);
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
                    },
                    validatePassword:(passwordValidateParams:SocketEventPasswordValidate) => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PasswordValidate),passwordValidateParams)
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
                    completePayment:() => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerCompletePayment))
                    },
                    goBankrupt:() => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerGoBankrupt))
                    },
                    payOutOfJail: () => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PayOutOfJail));
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
                    rollForUtilities: () => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerRollForUtilties))
                    },
                    setReadyStatus: (playerReadyParams:SocketEventPlayerReady) => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerReady),playerReadyParams);
                    },
                    update: (updateParams:SocketEventPlayerUpdate) => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PlayerUpdate),updateParams)
                    },
                },
                property:{
                    mortgage:(gamePropertyId:number) => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PropertyMortgage),gamePropertyId);
                    },
                    unmortgage:(gamePropertyId:number) => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PropertyUnmortgage),gamePropertyId);
                    },
                    upgrade:(gamePropertyId:number) => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PropertyUpgrade),gamePropertyId);
                    },
                    downgrade:(gamePropertyId:number) => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.PropertyDowngrade),gamePropertyId);
                    }
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
                    },
                    decline:(tradeDeclineParams:SocketEventTradeDecline) => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.TradeDecline),tradeDeclineParams)
                    },
                    accept:(tradeAcceptParams:SocketEventTradeAccept) => {
                        globalState.ws.invoke(getEnumNameFromValue(WebSocketEvents.TradeAccept),tradeAcceptParams)
                    }
                }
            },
            listen: (eventEnum:1|2|3|6|10|18) => {
                globalState.ws.on(eventEnum.toString(), webSocketCallbacks[eventEnum])
            },
            stopListen: (eventEnum:1|2|3|6|10|18) => {
                globalState.ws.off(eventEnum.toString(),webSocketCallbacks[eventEnum])
            },
        }
    },[])

    return webSocketCalls;
}