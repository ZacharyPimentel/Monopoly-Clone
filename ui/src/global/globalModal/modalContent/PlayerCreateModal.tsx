import { Fragment, useEffect, useState } from "react"
import { useApi } from "../../../hooks/useApi"
import { Player } from "../../../types/controllers/Player";
import { iconMappings } from "../../../helpers/IconMappings";
import { useGameState } from "../../../stateProviders/GameStateProvider";
import React from "react";
import { useGlobalDispatch, useGlobalState } from "../../../stateProviders/GlobalStateProvider";

export const PlayerCreateModal = () => {

    const api = useApi();
    const gameState = useGameState();
    const globalDispatch = useGlobalDispatch();
    const [inactivePlayers,setInactivePlayers] = useState<Player[]>([])
    const [selectedInactivePlayerId,setSelectedInactivePlayerId] = useState('');
    const [selectedIconId,setSelectedIconId] = useState(0)
    const [loading,setLoading] = useState(true);
    const [name,setName] = useState('');

    useEffect( () => {
        (async() => {
            const inactivePlayers = await api.player.search({isActive:false})
            setInactivePlayers(inactivePlayers);
            setLoading(false)
        })()
    },[gameState.players])

    return (
        <div className='flex flex-col gap-[20px]'>
            <div className='flex flex-col gap-[10px]'>
                <p className='font-bold'>Reconnect</p>
                {loading && (
                    <p>Loading inactive players...</p>
                )}
                {!loading && inactivePlayers.length === 0 && (
                    <p>There are no inactive players in the game.</p>
                )}
                {!loading && inactivePlayers.length > 0 && inactivePlayers.map( (player) => {
                    return (
                        <div className='flex items-center gap-[20px]' key={player.id}>
                            <button
                                onClick={() => setSelectedInactivePlayerId(player.id)} 
                                className={`px-[10px] w-fit flex gap-[10px] items-center ${selectedInactivePlayerId === player.id ? 'border border-black' : ''}`}>
                                <img className='w-[30px] h-[30px]' src={iconMappings.find( (mapping) => mapping.id === player.iconId)?.url}/>
                                {player.playerName}
                            </button>
                            <button 
                                onClick={() => {
                                    gameState.ws.invoke('Reconnectplayer',player.id);
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
                                setSelectedInactivePlayerId('')
                            }}} 
                            className={`border relative overflow-hidden ${mapping.id === selectedIconId ? 'border-black' : ''}`}>
                            <img className='' src={mapping.url}/>
                        </button>
                    )
                })}
            </div>
            <button
                onClick={() => {
                    gameState.ws.invoke("AddNewPlayer",name,selectedIconId);
                }}
                disabled={name === '' || selectedIconId === 0} 
                className='bg-totorodarkgreen text-white w-fit p-[10px] min-w-[100px] rounded mx-auto disabled:bg-[grey]'
            >
                Join
            </button>
        </div>
    )
}