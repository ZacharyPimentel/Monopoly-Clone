import { useWebSocket } from "@hooks";
import { Player } from "@generated"
import { PlayerEditModal } from "@globalComponents/GlobalModal/modalContent";
import { useGlobalState } from "@stateProviders";

export const CurrentPlayerInLobbyListItem:React.FC<{player:Player}> = ({player}) => {

    const {dispatch:globalDispatch} = useGlobalState([]);
    const {invoke} = useWebSocket();
    
    return (
        <li key={player.id} className='flex flex-col gap-[20px]'>
            <div className='flex items-center gap-[20px]'>

                <img className='w-[30px] h-[30px]' src={player.iconUrl}/>

                <p>{player.playerName} (You)</p>  
                <button onClick={() => globalDispatch({modalOpen:true,modalContent:<PlayerEditModal player={player}/>})}>
                    <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M200-200h57l391-391-57-57-391 391v57Zm-80 80v-170l528-527q12-11 26.5-17t30.5-6q16 0 31 6t26 18l55 56q12 11 17.5 26t5.5 30q0 16-5.5 30.5T817-647L290-120H120Zm640-584-56-56 56 56Zm-141 85-28-29 57 57-29-28Z"/></svg>
                </button>
                <button 
                    onClick={() => invoke.player.setReadyStatus({
                        isReadyToPlay:!player.isReadyToPlay
                    })}
                    className='ml-auto bg-totorolightgreen p-[5px] rounded'
                >
                    {player.isReadyToPlay ? 'Unready' : 'Ready'}
                </button>
            </div>
        </li>
    )
    
}