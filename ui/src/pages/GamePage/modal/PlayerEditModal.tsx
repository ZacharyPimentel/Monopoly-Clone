import { useState } from "react"
import { useApi } from "../../../hooks/useApi"
import { Player } from "../../../types/controllers/Player";
import { useGameState } from "../../../stateProviders/GameStateProvider";
import React from "react";
import { useGlobalDispatch} from "../../../stateProviders/GlobalStateProvider";
import { useWebSocket } from "../../../hooks/useWebSocket";
import { PlayerIcon } from "../../../types/controllers/PlayerIcon";
import { FetchWrapper } from "../../../globalComponents/FetchWrapper";
import { ActionButtons } from "../../../globalComponents/GlobalModal/ActionButtons";

export const PlayerEditModal:React.FC<{player:Player}> = ({player}) => {

    const api = useApi();
    const gameState = useGameState();
    const webSocket = useWebSocket();
    const globalDispatch = useGlobalDispatch();
    const [selectedIconId,setSelectedIconId] = useState(player.iconId);
    const [name,setName] = useState(player.playerName);
    const {invoke} = useWebSocket();

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Edit Player</p>
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
            <ActionButtons 
                confirmCallback={async() => {
                    invoke.player.update(player.id,{
                        iconId:selectedIconId,
                        playerName: name
                    })
                }} 
                confirmButtonStyle={"success"} 
                confirmButtonText={"Update"}
            />
        </div>
    )
}