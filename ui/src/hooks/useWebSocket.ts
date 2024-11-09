import { useGameState } from "../stateProviders/GameStateProvider"
import { Game } from "../types/controllers/Game";
import { PlayerUpdateParams } from "../types/controllers/Player";

export const useWebSocket = () => {
    const gameState = useGameState();
    
    return {
        //game events not tied to a table
        gameState:{
            setLastDiceRoll: (rolls:number[]) => {
                gameState.ws.invoke("SetLastDiceRoll",rolls)
            },
            updateRules: (rule:Partial<Game>) => {
                gameState.ws.invoke("UpdateRules",gameState.gameState?.id,rule)
            },
            endTurn: (gameId:number) => {
                gameState.ws.invoke("endTurn",gameId);
            }
        },
        player:{
            update: (playerId:string,updateParams:Partial<PlayerUpdateParams>) => {
                gameState.ws.invoke("UpdatePlayer",playerId,updateParams)
            }
        }
    }
}