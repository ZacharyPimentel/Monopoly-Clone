import { Player } from "../../../../../types/GameState";
import { useGameDispatch, useGameState } from "../../../../../global/GameStateProvider";
import { PlayerIcon } from "../../../../../global/PlayerIcon";
import { useMemo, useState } from "react";
import { EditPlayerModal } from "../../../../../global/globalModal/views/EditPlayerModal";
import { iconMappings } from "../../../../../helpers/IconMappings";

export const PlayerListItemInLobby:React.FC<{player:Player}> = ({player}) =>{

    const gameState = useGameState();
    const gameDispatch = useGameDispatch();

    const readyButtonClasses = useMemo( () => {
        if(player.isReady){
            return 'bg-totorored hover:bg-totorolightgreen text-black'
        }else{
            return 'bg-totorodarkgreen hover:bg-totorolightgreen'
        }
    },[player.isReady])

    const playerIconURL = useMemo( () => {
        const mapping = iconMappings.find( (mapping) => mapping.id === player.gamePieceID );
        return mapping?.url || '';
    },[player])

    //If the game hasn't started
    if(!gameState.gameInProgress){
        return (
            <li key={player.id} className={`flex flex-wrap items-center gap-[10px] p-[10px] rounded transition-[0.2s] bg-white`}>
                <PlayerIcon iconURL={playerIconURL} size={40} />
                <p className='font-bold text-ellipsis whitespace-nowrap overflow-hidden'>{player.nickName}</p>
                {/* allow users to edit their information */}
                {player.id === gameState.currentPlayer?.id && (<>
                    <button className='mr-auto' onClick={() => gameDispatch({modalOpen:true,modalContent:<EditPlayerModal/>})}>
                        <svg className='hover:fill-totorolightgreen duration-[0.2s]' xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M200-200h57l391-391-57-57-391 391v57Zm-80 80v-170l528-527q12-11 26.5-17t30.5-6q16 0 31 6t26 18l55 56q12 11 17.5 26t5.5 30q0 16-5.5 30.5T817-647L290-120H120Zm640-584-56-56 56 56Zm-141 85-28-29 57 57-29-28Z"/></svg>
                    </button>
                    <button 
                        className={`text-[14px] p-[5px] border-2 border-totorodarkgreen min-w-[90px] rounded text-white hover:text-black duration-[0.2s] ${readyButtonClasses}`}
                        onClick={( () => gameState.ws.invoke("UpdatePlayerReadyState",!player.isReady))}
                    >
                        {player.isReady ? "UNREADY" : "READY"}
                    </button>
                </>)}
                {player.id !== gameState.currentPlayer?.id && (
                    player.isReady
                    ?   <svg className='ml-auto' xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M382-240 154-468l57-57 171 171 367-367 57 57-424 424Z"/></svg>
                    :   <svg className='ml-auto' xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="m256-200-56-56 224-224-224-224 56-56 224 224 224-224 56 56-224 224 224 224-56 56-224-224-224 224Z"/></svg>
                )}
            </li>
        )
    }

    
}