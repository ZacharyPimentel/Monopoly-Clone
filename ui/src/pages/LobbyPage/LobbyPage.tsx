import { Fragment, useEffect } from "react";
import { useGlobalDispatch, useGlobalState } from "../../stateProviders/GlobalStateProvider"
import { GameCreateModal } from "./modal/CreateGameModal";
import { useWebSocket } from "../../hooks/useWebSocket";
import { useNavigate } from "react-router-dom";
import { WebSocketEvents } from "@generated/WebSocketEvents";
import { EnterPasswordModal } from "@globalComponents/GlobalModal/modalContent/EnterPasswordModal";
import { GameDeleteModal } from "@globalComponents/GlobalModal/modalContent/GameDeleteModal";

export const LobbyPage = () => {

    const globalDispatch = useGlobalDispatch();
    const {listen,stopListen,invoke} = useWebSocket();
    const navigate = useNavigate();
    const globalState = useGlobalState();

    useEffect( () => {
        listen(WebSocketEvents.GameUpdateAll)
        listen(WebSocketEvents.GameCreate);
        listen(WebSocketEvents.Error);
        listen(WebSocketEvents.PasswordValidated);
        invoke.game.getAll();
        return () => {
            stopListen(WebSocketEvents.GameUpdateAll)
            stopListen(WebSocketEvents.GameCreate)
            stopListen(WebSocketEvents.Error);
            stopListen(WebSocketEvents.PasswordValidated);

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
                    {globalState.availableGames.length > 0 && globalState.availableGames.map( (game) => {
                        return (<Fragment key={game.id}>
                            <li className='flex justify-between gap-[20px] items-center flex-wrap'>
                                <div className="flex gap-[10px] items-center">
                                    <p className='font-bold'>{game.gameName}</p>
                                    {game.hasPassword && (
                                        <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 -960 960 960" width="24px" fill="black"><path d="M240-80q-33 0-56.5-23.5T160-160v-400q0-33 23.5-56.5T240-640h40v-80q0-83 58.5-141.5T480-920q83 0 141.5 58.5T680-720v80h40q33 0 56.5 23.5T800-560v400q0 33-23.5 56.5T720-80H240Zm0-80h480v-400H240v400Zm240-120q33 0 56.5-23.5T560-360q0-33-23.5-56.5T480-440q-33 0-56.5 23.5T400-360q0 33 23.5 56.5T480-280ZM360-640h240v-80q0-50-35-85t-85-35q-50 0-85 35t-35 85v80ZM240-160v-400 400Z"/></svg> 
                                    )}
                                </div>
                                <button onClick={() => {
                                    if(game.hasPassword){
                                            globalDispatch({modalContent:<EnterPasswordModal gameId={game.id}/>, modalOpen:true})
                                    }else{
                                        navigate(`/game/${game.id}`)}
                                    }
                                } className='ml-auto bg-black text-white p-[10px] min-w-[100px]'>Join</button>
                                <p>{game.activePlayerCount} Active Player{game.activePlayerCount !== 1 && 's'}</p>
                                <div className='w-[20px] h-[20px]'>
                                    {game.activePlayerCount == 0 && (
                                        <button title="Delete Game" className='hover:fill-[tomato] transition'
                                            onClick={() => globalDispatch({modalContent:<GameDeleteModal game={game}/>, modalOpen:true})}
                                        >
                                            <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="m256-200-56-56 224-224-224-224 56-56 224 224 224-224 56 56-224 224 224 224-56 56-224-224-224 224Z"/></svg>
                                        </button>
                                    )}
                                </div>
                            </li>
                            <hr></hr>
                        </Fragment>)
                    })}
                    {globalState.availableGames.length === 0 && (
                        <p className='italic opacity-[0.5]'>No lobbies found.</p>
                    )}
                </ul>
            </div>
        </div>
    )
}