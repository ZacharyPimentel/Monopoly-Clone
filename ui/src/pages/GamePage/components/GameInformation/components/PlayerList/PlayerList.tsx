import { usePlayer } from "@hooks";
import { PlayerCreateModal } from "@globalComponents";
import { CurrentPlayerInLobbyListItem } from "./components/CurrentPlayerInLobbyListItem";
import { PlayerInGameListitem } from "./components/PlayerInGameListitem";
import { PlayerInLobbyListItem } from "./components/PlayerInLobbyListItem";
import { useGlobalState } from "@stateProviders";
import { useGameState } from "@stateProviders";

export const PlayerList = () => {

    const gameState = useGameState(['game','players','currentSocketPlayer']);
    const {dispatch:globalDispatch} = useGlobalState([]);
    const {player} = usePlayer();

    return (
        <div className='flex flex-col gap-[10px] p-[10px] bg-totorogreen'>
            {/* Valid players are players who have nickname / profile icon set */}
            <div className='flex justify-between gap-[20px] items-center'>
                <p className='font-bold'>Players</p>
                {!gameState.game?.gameStarted && !player && (
                    <button onClick={() => {
                        globalDispatch({modalContent:<PlayerCreateModal/>,modalOpen:true})
                    }} className='underline'>Open Player Creation</button>
                )}
            </div>
            {gameState.players.length > 0 && (
                <ul className='flex flex-col gap-[10px]'>
                    {gameState.players.sort((a,b) => a.turnOrder - b.turnOrder).map( (player) => {
                        const isCurrentPlayer = player.id === gameState.currentSocketPlayer?.playerId;
                        return (
                            gameState.game?.gameStarted 
                                ? player.inCurrentGame
                                    ? <PlayerInGameListitem player={player} key={player.id}/>
                                    : null
                                : isCurrentPlayer
                                    ? <CurrentPlayerInLobbyListItem key={player.id} player={player}/>
                                    : <PlayerInLobbyListItem key={player.id} player={player}/> 
                            
                        )
                    })}
                </ul>
            )}
            {/*If game has started, players not in game go into a lobby area in the ui instead */}
            {gameState.game?.gameStarted && (
                <div className='border-t border-black pt-[10px] flex flex-col gap-[10px]'>
                    <p className='font-bold'>Lobby</p>
                    {gameState.players.filter(player => !player.inCurrentGame).map( (player) => {
                        const isCurrentPlayer = player.id === gameState.currentSocketPlayer?.playerId;
                        return (
                            isCurrentPlayer
                                ? <CurrentPlayerInLobbyListItem key={player.id} player={player}/>
                                : <PlayerInLobbyListItem key={player.id} player={player}/>
                        )
                    })}
                </div>
            )}

            {/* Invalid players have just joined and don't have any info set */}
            {gameState.players.length === 0 && (
                <p className='text-center'>There are no players just yet.</p>
            )}
        </div>
    )
}