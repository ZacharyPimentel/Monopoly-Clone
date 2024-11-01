import { useState } from "react";
import { useGameState } from "../../../../stateProviders/GameStateProvider"
import { Player } from "../../../../types/GameState";

export const Navbar = () => {

    const gameState = useGameState();
    

    const [submenuOpen,setSubmenuOpen] = useState(false);

    return (
        <div className='flex justify-between w-full p-[10px] items-center relative bg-totorogreen md:hidden'>
            <ul className='flex flex-1'>
                {gameState.players.filter( (player) => !player.nickName).map ( (player:Player) => {
                    return (
                        <li key={player.id} className='flex items-center'>
                            <div className='rounded-[50%] border-2 border-black mr-[-10px] bg-totorogrey'>
                                <svg className='w-[25px] h-[25px]' xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M480-480q-66 0-113-47t-47-113q0-66 47-113t113-47q66 0 113 47t47 113q0 66-47 113t-113 47ZM160-160v-112q0-34 17.5-62.5T224-378q62-31 126-46.5T480-440q66 0 130 15.5T736-378q29 15 46.5 43.5T800-272v112H160Zm80-80h480v-32q0-11-5.5-20T700-306q-54-27-109-40.5T480-360q-56 0-111 13.5T260-306q-9 5-14.5 14t-5.5 20v32Zm240-320q33 0 56.5-23.5T560-640q0-33-23.5-56.5T480-720q-33 0-56.5 23.5T400-640q0 33 23.5 56.5T480-560Zm0-80Zm0 400Z"/></svg>        
                            </div>
                        </li>
                    )
                })}
            </ul>
            {/* <button onClick={() => setSubmenuOpen(!submenuOpen)} title='Expand Player List' className='flex flex-2 justify-center'>
                <svg className={`transition-[0.2s] ${submenuOpen ? 'scale-[-1]' : ''}`} xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M480-345 240-585l56-56 184 184 184-184 56 56-240 240Z"/></svg>
            </button> */}
            <ul className='flex flex-1 justify-end'>
                {gameState.players.filter( (player) => player.nickName).map ( (player:Player) => {
                    return (
                        <li key={player.id} className='flex items-center'>
                            <button disabled={submenuOpen} title={player.nickName || "View Player"} className='rounded-[50%] ml-[-10px] bg-white'>
                                <img className='w-[25px] h-[25px] ' src={player.gamePieceURL}/>
                            </button>
                        </li>
                    )
                })}
            </ul>
            {/* <SubmenuWrapper submenuOpen={submenuOpen} updateHeightDeps={gameState.players}>
                <PlayerList/>
            </SubmenuWrapper> */}
        </div>
    )
}