import { PlayerEditModal } from "../../../../../../globalComponents/GlobalModal/modalContent/PlayerEditModal";
import { useGameState } from "../../../../../../stateProviders/GameStateProvider";
import { useGlobalDispatch } from "../../../../../../stateProviders/GlobalStateProvider";
import { CurrentPlayerListItem } from "./components/CurrentPlayerListItem";
import { PlayerListItem } from "./components/PlayerListItem";

export const PlayerList = () => {

    const gameState = useGameState();
    const globalDispatch = useGlobalDispatch();
    
    return (
        <div className='flex flex-col gap-[10px] p-[10px] bg-totorogreen'>
            {/* Valid players are players who have nickname / profile icon set */}
            <p className='font-bold'>Players</p>
            {gameState.players.length > 0 && (
                <ul className='flex flex-col gap-[10px]'>
                    {gameState.players.map( (player) => {
                        const isCurrentPlayer = player.id === gameState.currentSocketPlayer?.playerId;
                        return (
                            isCurrentPlayer
                                ? <CurrentPlayerListItem key={player.id} player={player}/>
                                : <PlayerListItem key={player.id} player={player}/>
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