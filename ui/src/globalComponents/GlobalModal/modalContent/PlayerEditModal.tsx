import { useEffect } from "react"
import { useApi } from "@hooks/useApi"
import { Player, PlayerIcon, SocketEventPlayerEdit } from "@generated/index";
import { useGameState } from "@stateProviders/GameStateProvider";
import React from "react";
import { useWebSocket } from "@hooks/useWebSocket";
import { FetchWrapper } from "../../FetchWrapper";
import { ActionButtons } from "../ActionButtons";
import { useForm } from "react-hook-form";

export const PlayerEditModal:React.FC<{player:Player}> = ({player}) => {

    const api = useApi();
    const gameState = useGameState();
    const {invoke} = useWebSocket();

    const form = useForm<SocketEventPlayerEdit>({defaultValues:{
        playerName:player.playerName,
        iconId:player.iconId
    }})

    useEffect( () => {
        form.register('iconId');
    },[form.register])

    const newIconId = form.watch('iconId');
    const newPlayerName = form.watch('playerName');

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Edit Player</p>
            <label className='flex flex-wrap gap-[10px] items-center'>
                <p className='required min-w-[165px]'>Player Name:</p>
                <input 
                    {...form.register("playerName",{required:true})} 
                    className='text-input flex-1' 
                    type='text'
                />
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
                                        form.setValue('iconId',icon.id);
                                    }}} 
                                    className={`disabled:opacity-[0.5] border relative overflow-hidden ${icon.id === newIconId ? 'border-black' : ''}`}
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
                    //only update if something has actually changed
                    if(player.iconId !== newIconId || player.playerName !== newPlayerName){
                        invoke.player.edit({
                            ...(player.iconId !== newIconId && {iconId:newIconId}),
                            ...(player.playerName !== newPlayerName && {playerName:newPlayerName}),
                        })
                    }
                }} 
                confirmButtonStyle={"success"} 
                confirmButtonText={"Update"}
            />
        </div>
    )
}