import { useGameState } from "@stateProviders/GameStateProvider"
import { BoardSpace } from "@generated/index"

export const Jail:React.FC<{space:BoardSpace}> = ({space}) => {

    const {players,game} = useGameState();

    return (
        <div className='h-full bg-totorolightgreen shadow-lg border border-totorodarkgreen rounded-[5px] relative'>
            {/* Visiting Jail Players */}
            <ul className={`absolute top-0 left-[50%] translate-x-[-50%] flex space-x-[-5px] z-[1]`}>
                    {players.map((player) => {
                        if(player.boardSpaceId !== 11 ||player.inJail)return null
                        const isThisPlayersTurn = game?.currentPlayerTurn === player.id;
                        return (
                            <div key={player.id} className='relative' >
                                <span className={`${isThisPlayersTurn ? 'bg-pink-500 animate-ping' : ''} absolute w-full h-full rounded-[50%]`}></span>
                                <img className='w-[30px] h-[30px] bg-white border border-black rounded-[50%] relative' src={player.iconUrl}/>
                            </div>
                        )
                    })}
                </ul>
            <p style={{writingMode:'vertical-lr'}} className='ml-auto mt-[10px]'>Visiting</p>
            <div className='w-[75%] h-[75%] absolute bottom-0 left-0 border-t-2 border-r-2 rounded-tr-[5px] border-black flex justify-between flex-col items-center'>
                {/* In Jail Players */}
                <ul className={`absolute top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%] flex space-x-[-5px] z-[1]`}>
                    {players.map((player) => {
                        if(player.boardSpaceId !== 11 ||!player.inJail)return null
                        const isThisPlayersTurn = game?.currentPlayerTurn === player.id;
                        return (
                            <div key={player.id} className='relative' >
                                <span className={`${isThisPlayersTurn ? 'bg-pink-500 animate-ping' : ''} absolute w-full h-full rounded-[50%]`}></span>
                                <img className='w-[30px] h-[30px] bg-white border border-black rounded-[50%] relative' src={player.iconUrl}/>
                            </div>
                        )
                    })}
                </ul>
                <svg className='rotate-90 flex-1 h-full w-full'  xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M760-360v-80H200v80h560Zm0-160v-80H200v80h560Zm0-160v-80H200v80h560ZM200-120q-33 0-56.5-23.5T120-200v-560q0-33 23.5-56.5T200-840h560q33 0 56.5 23.5T840-760v560q0 33-23.5 56.5T760-120H200Zm560-80v-80H200v80h560Z"/></svg>
                <p>{space.boardSpaceName}</p>
            </div>
        </div>
    )
}