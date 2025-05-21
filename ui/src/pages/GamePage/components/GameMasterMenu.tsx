import { useState } from "react"
import { useGameMasterDispatch, useGameMasterState } from "../../../stateProviders/GameMasterStateProvider"

export const GameMasterMenu = () => {

    const [menuOpen,setMenuOpen] = useState(false)
    const gameMasterState = useGameMasterState();
    const gameMasterDispatch = useGameMasterDispatch();

    return (
        
        <div className='fixed bottom-[20px] right-[50px] z-[10]'>
            {menuOpen && (
                <div className='absolute bg-white bottom-full right-0 min-w-[100px] min-h-[100px] p-[10px] flex flex-col gap-[20px]'>
                    <label>
                        <p>Force Landed On Space</p>
                        <input
                            value={gameMasterState.forceLandedSpace}
                            className='border border-black rounded' 
                            onChange={(e) => {
                                if(e.target.value){
                                    gameMasterDispatch({forceLandedSpace:parseInt(e.target.value)})
                                }else{
                                    gameMasterDispatch({forceLandedSpace:0})
                                }
                            }} 
                            type='number'
                        />
                    </label>
                    <label>
                        <p>Force Next Chance Card Id</p>
                        <input
                            value={gameMasterState.forceNextCardId}
                            className='border border-black rounded' 
                            onChange={(e) => {
                                if(e.target.value){
                                    gameMasterDispatch({forceNextCardId:parseInt(e.target.value)})
                                }else{
                                    gameMasterDispatch({forceNextCardId:0})
                                }
                            }} 
                            type='number'
                        />
                    </label>
                    <label>
                        <p>Force Next Dice One</p>
                        <input
                            value={gameMasterState.forceDiceOne}
                            className='border border-black rounded' 
                            onChange={(e) => {
                                if(e.target.value){
                                    gameMasterDispatch({forceDiceOne:parseInt(e.target.value)})
                                }else{
                                    gameMasterDispatch({forceDiceOne:0})
                                }
                            }} 
                            type='number'
                        />
                    </label>
                    <label>
                        <p>Force Next Dice Two</p>
                        <input
                            value={gameMasterState.forceDiceTwo}
                            className='border border-black rounded' 
                            onChange={(e) => {
                                if(e.target.value){
                                    gameMasterDispatch({forceDiceTwo:parseInt(e.target.value)})
                                }else{
                                    gameMasterDispatch({forceDiceTwo:0})
                                }
                            }} 
                            type='number'
                        />
                    </label>
                </div>
            )}
            <button onClick={( () => setMenuOpen(!menuOpen))} className='bg-white  rounded-[50%] border border-black h-[50px] w-[50px] flex items-center justify-center'>
                <svg className='w-[30px] h-[30px] fill-black' xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 -960 960 960" width="24px" fill="#5f6368"><path d="M710-150q-63 0-106.5-43.5T560-300q0-63 43.5-106.5T710-450q63 0 106.5 43.5T860-300q0 63-43.5 106.5T710-150Zm0-80q29 0 49.5-20.5T780-300q0-29-20.5-49.5T710-370q-29 0-49.5 20.5T640-300q0 29 20.5 49.5T710-230Zm-550-30v-80h320v80H160Zm90-250q-63 0-106.5-43.5T100-660q0-63 43.5-106.5T250-810q63 0 106.5 43.5T400-660q0 63-43.5 106.5T250-510Zm0-80q29 0 49.5-20.5T320-660q0-29-20.5-49.5T250-730q-29 0-49.5 20.5T180-660q0 29 20.5 49.5T250-590Zm230-30v-80h320v80H480Zm230 320ZM250-660Z"/></svg>
            </button>
        </div>
    )
}