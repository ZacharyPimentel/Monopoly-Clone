import { Fragment, useEffect, useState } from "react"
import { useApi } from "../../../hooks/useApi"
import { Player } from "../../../types/controllers/Player";
import { iconMappings } from "../../../helpers/IconMappings";
import { useGameState } from "../../../stateProviders/GameStateProvider";
import React from "react";
import { useGlobalDispatch, useGlobalState } from "../../../stateProviders/GlobalStateProvider";
import { ActionButtons } from "../ActionButtons";
import { useWebSocket } from "../../../hooks/useWebSocket";

export const PlayerEditModal:React.FC<{player:Player}> = ({player}) => {

    const api = useApi();
    const gameState = useGameState();
    const webSocket = useWebSocket();
    const globalDispatch = useGlobalDispatch();
    const [selectedIconId,setSelectedIconId] = useState(player.iconId);
    const [name,setName] = useState(player.playerName);

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Edit Player</p>
            <label className='flex flex-wrap gap-[10px] items-center'>
                <p className='required min-w-[165px]'>Player Name:</p>
                <input value={name} onChange={(e) => setName(e.target.value)} className='text-input flex-1' type='text'/>
            </label>
            <p className='required'>Player Icon</p>
            <div className='grid grid-cols-3 md:grid-cols-4 gap-[20px] overflow-y-scroll custom-scrollbar max-h-[400px]'>
                {iconMappings.filter( (mapping) => {
                    console.log(gameState.players)
                    return gameState.players.length === 0 
                        ? true
                        : gameState.players.some( (player) => player.iconId !== mapping.id)}
                ).map( (mapping) => {
                    return (
                        <button 
                            key={mapping.id}
                            onClick={()=> {{
                                setSelectedIconId(mapping.id)
                            }}} 
                            className={`border relative overflow-hidden ${mapping.id === selectedIconId ? 'border-black' : ''}`}>
                            <img className='' src={mapping.url}/>
                        </button>
                    )
                })}
            </div>
            <ActionButtons 
                confirmCallback={async() => {
                    webSocket.player.update(player.id,{
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