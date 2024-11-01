import { useGameState } from "../../../../stateProviders/GameStateProvider";
import { useGlobalDispatch } from "../../../../stateProviders/GlobalStateProvider";
import { PlayerEditModal } from "../../../../globalComponents/GlobalModal/modalContent/PlayerEditModal";

export const PlayerList = () => {

    const gameState = useGameState();
    const globalDispatch = useGlobalDispatch();
    
    return (
        <div className='flex flex-col gap-[10px] p-[10px] bg-totorogreen'>
            {/* Valid players are players who have nickname / profile icon set */}
            {gameState.players.length > 0 && (
                <ul className='flex flex-col gap-[10px]'>
                    {gameState.players.map( (player) => {
                        const isCurrentPlayer = player.id === gameState.currentSocketPlayer?.playerId;
                        return (
                            <li style={{opacity:player.active ? '1' : '0.5'}} key={player.id} className='flex flex-col gap-[20px]'>
                                <div className='flex items-center gap-[20px]'>
                                    <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                                    <p>{player.playerName} {isCurrentPlayer ? '(You)' : ''}</p>
                                    {isCurrentPlayer && (<>
                                        <button onClick={() => globalDispatch({modalOpen:true,modalContent:<PlayerEditModal player={player}/>})}>
                                            <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M200-200h57l391-391-57-57-391 391v57Zm-80 80v-170l528-527q12-11 26.5-17t30.5-6q16 0 31 6t26 18l55 56q12 11 17.5 26t5.5 30q0 16-5.5 30.5T817-647L290-120H120Zm640-584-56-56 56 56Zm-141 85-28-29 57 57-29-28Z"/></svg>
                                        </button>
                                        <button className='ml-auto bg-totorolightgreen p-[5px] rounded'>Ready</button>
                                    </>)}
                                    
                                </div>
                            </li>
                        )
                    })}
                    {/* {validPlayers.map( (player) => {
                        return (<React.Fragment key={player.id}>
                            {gameState.gameInProgress
                                ? <PlayerListItemInGame player={player}/>
                                : <PlayerListItemInLobby player={player}/>
                            }
                        </React.Fragment>)
                    })} */}
                </ul>
            )}
            {/* Invalid players have just joined and don't have any info set */}
            {gameState.players.length === 0 && (
                <p className='text-center'>There are no players just yet.</p>
            )}
        </div>
    )
}