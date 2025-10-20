import { Card } from "../types/controllers/Card";
import { Player, PlayerUpdateParams, PlayerWhereParams, PlayerIcon, BoardSpace, GameLog } from "@generated";
import { Theme } from "../types/controllers/Theme";
import { useHttp } from "@hooks";

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
            },
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
        card:{
            find: async(cardId:number):Promise<Card> => {
                return await request.get(`${apiUrl}/card/${cardId}`)
            }
        },
        gameCard:{
            getOne: async(gameId:string,cardTypeId:number) => {
                return await request.get(`${apiUrl}/gameCard/getone`,{gameId,cardTypeId})
            }
        },
        gameLog:{
            getAll: async(gameId:string):Promise<GameLog[]> => {
                return await request.get(`${apiUrl}/gameLog`,{gameId})
            }
        }
    }
}