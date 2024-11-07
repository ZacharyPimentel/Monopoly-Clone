import { Player } from "../../../../../../../types/controllers/Player"

export const PlayerInGameListitem:React.FC<{player:Player}> = ({player}) => {
    return (
        <li style={{opacity:player.active ? '1' : '0.5'}} key={player.id} className='flex flex-col gap-[20px]'>
            <div className='flex items-center gap-[20px]'>
                <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                <p>{player.playerName}</p>
                <p className='ml-auto'>${player.money}</p>
            </div>
        </li>
    )
}