import { useGameState } from "../stateProviders/GameStateProvider"
import { Game } from "../types/controllers/Game";
import { PlayerUpdateParams } from "../types/controllers/Player";

export const useWebSocket = () => {
    const gameState = useGameState();
    
    return {
        gameState:{
            setLastDiceRoll: (rolls:number[]) => {
                gameState.ws.invoke("SetLastDiceRoll",rolls)
            },
            updateRules: (rule:Partial<Game>) => {
                gameState.ws.invoke("UpdateRules",gameState.gameState?.id,rule)
            }
        },
        player:{
            update: (playerId:string,updateParams:Partial<PlayerUpdateParams>) => {
                gameState.ws.invoke("UpdatePlayer",playerId,updateParams)
            }
        }
    }
}