import { useGameState } from "../stateProviders/GameStateProvider"
import { useGlobalState } from "../stateProviders/GlobalStateProvider";
import { Game } from "../types/controllers/Game";
import { PlayerUpdateParams } from "../types/controllers/Player";
import { PropertyUpdateParams } from "../types/controllers/Property";
import { GameUpdateParams } from "../types/websocket/Game";

export const useWebSocket = () => {
    const globalState = useGlobalState();
    const gameState = useGameState();
    return {
        invoke:{
            //game events not tied to a table
            gameState:{
                setLastDiceRoll: (rolls:number[]) => {
                    globalState.ws.invoke("SetLastDiceRoll",rolls)
                },
                updateRules: (rule:Partial<Game>) => {
                    globalState.ws.invoke("UpdateRules",gameState.gameState?.id,rule)
                },
                endTurn: (gameId:number) => {
                    globalState.ws.invoke("endTurn",gameId);
                }
            },
            game:{
                getAll: () => {
                    globalState.ws.invoke('GameGetAll');
                },
                getById: (gameId:string) => {
                    globalState.ws.invoke('GameGetById',gameId)
                },
                getLobbies: () => {
                    globalState.ws.invoke("GameGetLobbies");
                },
                create: (gameName:string) => {
                    globalState.ws.invoke('GameCreate',{gameName});
                },
                join: (gameId:string) => {
                    globalState.ws.invoke('GameJoin',gameId)
                },
                leave: (gameId:string) => {
                    globalState.ws.invoke('GameLeave',gameId);
                },
                update: (gameId:string,gameUpdateParams:GameUpdateParams) => {
                    globalState.ws.invoke('GameUpdate',gameId,gameUpdateParams);
                }
            },
            lastDiceRoll:{
                update: (gameId:string,diceOne:number,diceTwo:number) => {
                    globalState.ws.invoke('LastDiceRollUpdate',gameId,diceOne,diceTwo)
                }
            },
            player:{
                getAll: () => {
                    globalState.ws.invoke("PlayerGetAll")
                },
                update: (playerId:string,updateParams:Partial<PlayerUpdateParams>) => {
                    globalState.ws.invoke("PlayerUpdate",playerId,updateParams)
                },
                reconnect: (playerId:string) => {
                    globalState.ws.invoke('PlayerReconnect',playerId)
                },
                create:(playerName:string,iconId:number,gameId:string) => {
                    globalState.ws.invoke('PlayerCreate',{playerName,iconId,gameId})
                }
            },
            property:{
                update:(propertyId:number,updateParams:Partial<PropertyUpdateParams>) => {
                    globalState.ws.invoke("PropertyUpdate",propertyId,updateParams)
                }
            }
        },
        listen: (eventName:string, callback:(data:any) => void) => {
            globalState.ws.on(eventName,callback)
        },
        stopListen: (eventName:string, callback:(data:any) => void) => {
            globalState.ws.off(eventName,callback)
        }

    }
}