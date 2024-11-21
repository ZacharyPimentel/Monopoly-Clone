import { useGameState } from "../../../../../../stateProviders/GameStateProvider";
import { CurrentPlayerInLobbyListItem } from "./components/CurrentPlayerInLobbyListItem";
import { PlayerInGameListitem } from "./components/PlayerInGameListitem";
import { PlayerInLobbyListItem } from "./components/PlayerInLobbyListItem";

export const PlayerList = () => {

    const gameState = useGameState();
    
    return (
        <div className='flex flex-col gap-[10px] p-[10px] bg-totorogreen'>
            {/* Valid players are players who have nickname / profile icon set */}
            <p className='font-bold'>Players</p>
            {gameState.players.length > 0 && (
                <ul className='flex flex-col gap-[10px]'>
                    {gameState.players.map( (player) => {
                        const isCurrentPlayer = player.id === gameState.currentSocketPlayer?.playerId;
                        return (
                            gameState.game?.gameStarted 
                                ? <PlayerInGameListitem player={player} key={player.id}/>
                                : isCurrentPlayer
                                    ? <CurrentPlayerInLobbyListItem key={player.id} player={player}/>
                                    : <PlayerInLobbyListItem key={player.id} player={player}/>
                        )
                    })}
                </ul>
            )}

            {/* Invalid players have just joined and don't have any info set */}
            {gameState.players.length === 0 && (
                <p className='text-center'>There are no players just yet.</p>
            )}
        </div>
    )
}