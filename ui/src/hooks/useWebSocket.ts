import { useGameState } from "../stateProviders/GameStateProvider"
import { PlayerUpdateParams } from "../types/controllers/Player";

export const useWebSocket = () => {
    const gameState = useGameState();
    
    return {
        gameState:{
            setLastDiceRoll: (rolls:number[]) => {
                gameState.ws.invoke("SetLastDiceRoll",rolls)
            }
        },
        player:{
            update: (playerId:string,updateParams:Partial<PlayerUpdateParams>) => {
                gameState.ws.invoke("UpdatePlayer",playerId,updateParams)
            }
        }
    }
}