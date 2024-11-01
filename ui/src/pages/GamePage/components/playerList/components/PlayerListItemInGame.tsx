import { useMemo } from "react";
import { Player } from "../../../../../types/GameState";
import { useGameState } from "../../../../../stateProviders/GameStateProvider";
import { PlayerIcon } from "../../../../../globalComponents/PlayerIcon";

export const PlayerListItemInGame:React.FC<{player:Player}> = ({player}) =>{

    const gameState = useGameState();

    const bgClass = useMemo ( () => {
        console.log(player)
        if(!player.isConnected){
            return 'bg-totorored'
        }
        return 'bg-totorolightgreen'
    },[player])

    //If the game is in progress
    if(gameState.gameInProgress){
        return (
            <li key={player.id} className={`flex items-center gap-[20px] py-[10px] px-[15px] rounded transition-[0.2s] ${bgClass} relative`}>
                <span className={`w-[10px] h-full absolute rounded left-0 duration-[0.2s] ${player.isMyTurn ? 'bg-totoroyellow' : 'bg-transparent'}`}></span>
                <PlayerIcon iconURL={player.iconURL} size={40} />
                <p className='font-bold'>{player.nickName}</p>
                <p className='font-bold ml-auto'>${player.money}</p>
            </li>
        )
    }
}