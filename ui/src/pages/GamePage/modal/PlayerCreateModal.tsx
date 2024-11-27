import { useMemo, useState } from "react"
import { useApi } from "../../../hooks/useApi"
import { useGameState } from "../../../stateProviders/GameStateProvider";
import { useGlobalDispatch } from "../../../stateProviders/GlobalStateProvider";
import { PlayerIcon } from "../../../types/controllers/PlayerIcon";
import { useWebSocket } from "../../../hooks/useWebSocket";
import { FetchWrapper } from "../../../globalComponents/FetchWrapper";

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
        <div className='flex flex-col gap-[20px]'>
            <div className='flex flex-col gap-[10px]'>
                <p className='font-bold'>Reconnect</p>
                {inactivePlayers.length === 0 && (
                    <p>There are no inactive players in the game.</p>
                )}
                {inactivePlayers.length > 0 && inactivePlayers.map( (player) => {
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
            
            <p className='font-bold'>Create A Player</p>
            <label className='flex flex-wrap gap-[10px] items-center'>
                <p className='required min-w-[165px]'>Player Name:</p>
                <input value={name} onChange={(e) => setName(e.target.value)} className='text-input flex-1' type='text'/>
            </label>
            <p className='required'>Player Icons</p>
            <div className='grid grid-cols-3 md:grid-cols-4 gap-[20px] overflow-y-scroll custom-scrollbar max-h-[400px]'>
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
                                    className={`disabled:opacity-[0.5] border relative overflow-hidden ${icon.id === selectedIconId ? 'border-black' : ''}`}
                                    >
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
                className='bg-totorodarkgreen text-white w-fit p-[10px] min-w-[100px] rounded mx-auto disabled:bg-[grey]'
            >
                Join
            </button>
        </div>
    )
}