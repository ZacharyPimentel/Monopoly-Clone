import { useState } from "react";
import { useGameDispatch, useGameState } from "../stateProviders/GameStateProvider";
import { iconMappings } from "../helpers/IconMappings";
import { GameState } from "../types/GameState";

export const PlayerInfoForm = () => {
    const gameState = useGameState();
    const gameDispatch = useGameDispatch();
    const [nameInput,setNameInput] = useState(gameState.currentPlayer?.nickName || '');
    const [selectedIconMapping,setSelectedIconMapping] = useState<{id:number,url:string}>({
        id: gameState.currentPlayer?.gamePieceID || 0,
        url: gameState.currentPlayer?.gamePieceURL || ''
    })

    return (
        <div className='flex flex-col gap-[20px]'>
            <input placeholder="20 characters maximum" className='border border-black p-[5px] text-center rounded-[20px] w-full' 
                type='text' 
                value={nameInput} 
                onChange={(e) => {
                    if(e.target.value.length > 20)return
                    setNameInput(e.target.value)
                }} 
            />
            <div className='grid grid-cols-3 md:grid-cols-4 gap-[20px] overflow-y-scroll custom-scrollbar max-h-[400px]'>
                {iconMappings.map( (mapping) => {
                    return (
                        <button 
                            key={mapping.id}
                            onClick={()=> {{setSelectedIconMapping(mapping)}}} 
                            className={`border relative overflow-hidden ${mapping.id === selectedIconMapping.id ? 'border-black' : ''}`}>
                            <img className='' src={mapping.url}/>
                        </button>
                    )
                })}
            </div>
            <button 
                disabled={!nameInput || !selectedIconMapping.id || !selectedIconMapping.url} 
                onClick={() => {updatePlayerInfo()}}
                className='w-fit border border-black rounded px-[10px] py-[5px] mx-auto'>
                    READY
            </button>
        </div>
    )
    function updatePlayerInfo(){
        gameState.ws?.invoke("UpdatePlayerNameAndGamePiece",nameInput,selectedIconMapping.id,selectedIconMapping.url);
        gameDispatch({
            modalOpen:false,
            modalContent:null
        })
    }
}