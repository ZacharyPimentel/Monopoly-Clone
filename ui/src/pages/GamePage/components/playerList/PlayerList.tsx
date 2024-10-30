import { useMemo } from "react";
import { Player } from "../../../../types/GameState";
import { useGameState } from "../../../../global/GameStateProvider";
import React from "react";
import { PlayerListItemInGame } from "./components/PlayerListItemInGame";
import { PlayerListItemInLobby } from "./components/PlayerListItemLobby";

export const PlayerList = () => {

    const gameState = useGameState();

    const validPlayers:Player[] = useMemo( () => {
        return gameState.players.filter( (player) => player.nickName);
    },[gameState.players])
    
    return (
        <div className='flex flex-col gap-[10px] p-[10px] bg-totorogreen'>
            {/* Valid players are players who have nickname / profile icon set */}
            {validPlayers.length > 0 && (
                <ul className='flex flex-col gap-[10px]'>
                    {validPlayers.map( (player) => {
                        return (<React.Fragment key={player.id}>
                            {gameState.gameInProgress
                                ? <PlayerListItemInGame player={player}/>
                                : <PlayerListItemInLobby player={player}/>
                            }
                        </React.Fragment>)
                    })}
                </ul>
            )}
            {/* Invalid players have just joined and don't have any info set */}
            {validPlayers.length === 0 && (
                <p className='text-center'>There are no players just yet.</p>
            )}
        </div>
    )
}