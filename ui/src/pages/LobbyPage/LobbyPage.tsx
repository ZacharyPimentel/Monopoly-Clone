import { Fragment, useEffect, useState } from "react";
import { useGlobalDispatch } from "../../stateProviders/GlobalStateProvider"
import { GameCreateModal } from "./modal/CreateGameModal";
import { useWebSocket } from "../../hooks/useWebSocket";
import { useNavigate } from "react-router-dom";
import { LobbyGame } from "../../types/websocket/Game";
import { WebSocketEvents } from "@generated/WebSocketEvents";
import { toast } from "react-toastify";



export const LobbyPage = () => {

    const globalDispatch = useGlobalDispatch();
    const {listen,stopListen,invoke} = useWebSocket();
    const [games,setGames] = useState<LobbyGame[]>([])
    const navigate = useNavigate();

    useEffect( () => {
        const gameListCallback = (games:LobbyGame[]) => setGames(games);
        const gameCreateCallback = (gameId:string) => navigate(`/game/${gameId}`);
        const errorCallback = (message:string) => {
            toast(message,{type:'error'})
        }

        listen(WebSocketEvents.GameUpdateAll,gameListCallback)
        listen(WebSocketEvents.GameCreate,gameCreateCallback);
        listen(WebSocketEvents.Error,errorCallback);

        invoke.game.getAll();
        
        return () => {
            stopListen(WebSocketEvents.GameUpdateAll,gameListCallback)
            stopListen(WebSocketEvents.GameCreate,gameCreateCallback)
            stopListen(WebSocketEvents.Error,errorCallback);
        }
    },[])

    return (
        <div className='custom-breakpoint-container py-[20px]'>
            <div className='bg-white p-[20px] flex flex-wrap items-center justify-between gap-[20px]'>
                <p className='font-bold'>Welcome To Monopoly!</p>
                <button onClick={() => globalDispatch({modalOpen:true,modalContent:<GameCreateModal/>})} className=' p-[10px] bg-black text-white'>Create New Game</button>
            </div>
            <div className='bg-white p-[20px] gap-[20px] flex flex-col'>
                <p>Existing Games:</p>
                <hr></hr>
                <ul className='flex flex-col gap-[20px]'>
                    {games.length > 0 && games.map( (game) => {
                        return (<Fragment key={game.id}>
                            <li className='flex justify-between gap-[20px] items-center flex-wrap'>
                                <p className='font-bold'>{game.gameName}</p>
                                <button onClick={() => navigate(`/game/${game.id}`)} className='ml-auto bg-black text-white p-[10px] min-w-[100px]'>Join</button>
                                <p>{game.activePlayerCount} Active Player{game.activePlayerCount > 1 && 's'}</p>
                            </li>
                            <hr></hr>
                        </Fragment>)
                    })}
                    {games.length === 0 && (
                        <p className='italic opacity-[0.5]'>No lobbies found.</p>
                    )}
                </ul>
            </div>
        </div>
    )
}