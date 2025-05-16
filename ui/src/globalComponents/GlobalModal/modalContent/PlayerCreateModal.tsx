import { useMemo, useState } from "react"
import { useApi } from "../../../hooks/useApi"
import { useGameState } from "../../../stateProviders/GameStateProvider";
import { useGlobalDispatch } from "../../../stateProviders/GlobalStateProvider";
import { PlayerIcon } from "../../../types/controllers/PlayerIcon";
import { useWebSocket } from "../../../hooks/useWebSocket";
import { FetchWrapper } from "../../FetchWrapper";

export const PlayerCreateModal = () => {

    const api = useApi();
    const gameState = useGameState();
    const globalDispatch = useGlobalDispatch();
    const [selectedInactivePlayerId,setSelectedInactivePlayerId] = useState('');
    const [selectedIconId,setSelectedIconId] = useState(0)
    const [name,setName] = useState('');
    const {invoke} = useWebSocket();

    const inactivePlayers = useMemo( () => 
        gameState.players.filter((player) => !player.active)
    ,[gameState.players])

    return (
        <div className='flex flex-col gap-6'>
            {inactivePlayers.length > 0 && (
                <div className='flex flex-col gap-[10px]'>
                    <p className='font-bold'>Reconnect</p>
                    {inactivePlayers.map( player => {
                        return (
                            <div className='flex items-center gap-[20px]' key={player.id}>
                                <button
                                    onClick={() => setSelectedInactivePlayerId(player.id)} 
                                    className={`px-[10px] w-fit flex gap-[10px] items-center ${selectedInactivePlayerId === player.id ? 'border border-black' : ''}`}>
                                    <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                                    {player.playerName}
                                </button>
                                <button 
                                    onClick={() => {
                                        invoke.player.reconnect(player.id);
                                        globalDispatch({modalContent:null,modalOpen:false})
                                    }}
                                    className='bg-totorodarkgreen text-white w-fit p-[10px] min-w-[100px] rounded disabled:bg-[grey]'>
                                    Reconnect
                                </button>
                            </div>
                        )
                    })}
                </div>
            )}
            
            <p className='font-bold mb-5'>Create A Player</p>
            <input 
                value={name} 
                onChange={(e) => setName(e.target.value)} 
                className='text-input flex-1' 
                type='text'
                placeholder="Enter Your Name"
            />
            <div className='grid grid-cols-3 md:grid-cols-4 gap-5 max-h-[400px]'>
                <FetchWrapper
                    apiCall={async() => api.playerIcon.getAll()}
                >
                    {(data:PlayerIcon[]) => 
                        data.map( (icon) => {
                            return (
                                <button 
                                    key={icon.id}
                                    disabled={gameState.players.find( (player) => player.iconId === icon.id) ? true : false}
                                    onClick={()=> {{
                                        setSelectedIconId(icon.id)
                                        setSelectedInactivePlayerId('')
                                    }}} 
                                    className={`disabled:opacity-[0.4] enabled:hover:scale-[1.1] transition relative overflow-hidden`}
                                    >
                                    {icon.id === selectedIconId && gameState.players.find( (player) => player.iconId !== icon.id) && (
                                        <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 -960 960 960" width="24px" fill="#5f6368"><path d="m424-296 282-282-56-56-226 226-114-114-56 56 170 170Zm56 216q-83 0-156-31.5T197-197q-54-54-85.5-127T80-480q0-83 31.5-156T197-763q54-54 127-85.5T480-880q83 0 156 31.5T763-763q54 54 85.5 127T880-480q0 83-31.5 156T763-197q-54 54-127 85.5T480-80Zm0-80q134 0 227-93t93-227q0-134-93-227t-227-93q-134 0-227 93t-93 227q0 134 93 227t227 93Zm0-320Z"/></svg>
                                    )}
                                    <img className='' src={icon.iconUrl}/>
                                    
                                </button>
                            )
                        })
                    }
                </FetchWrapper>
            </div>
            <button
                onClick={() => {
                    invoke.player.create(name,selectedIconId,gameState.game!.id);
                    globalDispatch({modalOpen:false,modalContent:null})
                }}
                disabled={name === '' || selectedIconId === 0} 
                className='bg-primary6 text-white w-fit p-[10px] min-w-[100px] rounded mx-auto disabled:bg-[grey]'
            >
                Join Game
            </button>
        </div>
    )
}