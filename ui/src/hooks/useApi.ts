import { BoardSpace } from "../types/controllers/BoardSpace";
import { Player, PlayerUpdateParams, PlayerWhereParams } from "../types/controllers/Player";
import { PlayerIcon } from "../types/controllers/PlayerIcon";
import { Theme } from "../types/controllers/Theme";
import { useHttp } from "./useHttp";

export const useApi = () => {

    const request = useHttp();
    const apiUrl = import.meta.env.VITE_API_URL;

    return {
        boardSpace:{
            getAll: async():Promise<BoardSpace[]> => request.get(`${apiUrl}/boardSpace`),
        },
        player:{
            search: async(whereParams:Partial<PlayerWhereParams>):Promise<Player[]> => {
                return await request.get(`${apiUrl}/player`,whereParams)
            },
            update: async(playerId:string,updateParams:Partial<PlayerUpdateParams>) => {
                return await request.patch(`${apiUrl}/player/${playerId}`,updateParams)
            }
        },
        playerIcon:{
            getAll: async():Promise<PlayerIcon[]> => {
                return await request.get(`${apiUrl}/playerIcon`);
            }
        },
        theme:{
            getAll: async():Promise<Theme[]> => {
                return await request.get(`${apiUrl}/theme`);
            }
        },
        gameCard:{
            getOne: async(gameId:string,cardTypeId:number) => {
                console.log(gameId,cardTypeId)
                return await request.get(`${apiUrl}/gameCard/getone`,{gameId,cardTypeId})
            }
        }
    }
}