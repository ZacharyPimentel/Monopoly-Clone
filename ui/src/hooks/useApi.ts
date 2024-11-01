import { BoardSpace } from "../types/controllers/BoardSpace";
import { Player, PlayerWhereParams } from "../types/controllers/Player";
import { PlayerIcon } from "../types/controllers/PlayerIcon";
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
            }
        },
        playerIcon:{
            getAll: async():Promise<PlayerIcon[]> => {
                return await request.get(`${apiUrl}/playerIcon`);
            }
        }
    }
}