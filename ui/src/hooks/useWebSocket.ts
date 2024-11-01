import { useGameState } from "../stateProviders/GameStateProvider"
import { PlayerUpdateParams } from "../types/controllers/Player";

export const useWebSocket = () => {
    const gameState = useGameState();
    
    return {
        player:{
            update: async(playerId:string,updateParams:PlayerUpdateParams) => {
                gameState.ws.invoke("UpdatePlayer",playerId,updateParams)
            }
        }
    }
}