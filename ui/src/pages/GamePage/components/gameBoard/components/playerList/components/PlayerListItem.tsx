import { Player } from "../../../../../../../types/controllers/Player"

export const PlayerListItem:React.FC<{player:Player}> = ({player}) => {
    return (
        <li style={{opacity:player.active ? '1' : '0.5'}} key={player.id} className='flex flex-col gap-[20px]'>
            <div className='flex items-center gap-[20px]'>
                <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                <p>{player.playerName}</p>
                {player.isReadyToPlay 
                    ? 
                        <div className='flex gap-[5px] ml-auto items-center'>
                            <p>Ready!</p>
                            <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M382-240 154-468l57-57 171 171 367-367 57 57-424 424Z"/></svg>
                        </div>
                    :
                        <p className='ml-auto opacity-[0.7] italic'>Not Ready</p>
                }
                
            </div>
        </li>
    )
}