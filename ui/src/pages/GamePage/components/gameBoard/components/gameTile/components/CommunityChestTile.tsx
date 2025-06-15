import { useState, useEffect, useMemo } from "react";
import { usePlayer } from "@hooks/usePlayer";
import { useGameState, useGameDispatch } from "@stateProviders/GameStateProvider";
import { BoardSpace } from "@generated/index"

export const CommunityChestTile:React.FC<{space:BoardSpace}> = ({space}) => {

    const {cardToastMessage} = useGameState();
    const gameDispatch = useGameDispatch();
    const [toastVisible,setToastVisible] = useState(false);
    const {player} = usePlayer();

    useEffect( () => {
        if(!cardToastMessage){
            setToastVisible(false)
            return
        }
        //only show the toast on the space that the player is actually on
        if(player.boardSpaceId !== space.id) return

        setToastVisible(true)
        setTimeout( () => {
            setToastVisible(false)
            gameDispatch({cardToastMessage:''})
        },3000)
    },[player,cardToastMessage])

    const positionStyles = useMemo( () => {
        switch(space.id){
            case 3:{
                return 'absolute top-[100%] left-[50%] translate-x-[-50%]'
            }
            case 18:{
                return 'right-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]'
            }
            case 34:{
                return 'left-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]'
            }
        }
    },[space])
    return (
        <div className='h-full relative'>
            <div className='bg-[orange] flex flex-col items-center justify-center h-full text-center w-full shadow-lg border border-totorodarkgreen rounded-[5px]'>
                <p className='text-[12px]'>{space.boardSpaceName}</p>
                <svg className=' w-[50%] opacity-[30%]' xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M640-520q17 0 28.5-11.5T680-560q0-17-11.5-28.5T640-600q-17 0-28.5 11.5T600-560q0 17 11.5 28.5T640-520Zm-320-80h200v-80H320v80ZM180-120q-34-114-67-227.5T80-580q0-92 64-156t156-64h200q29-38 70.5-59t89.5-21q25 0 42.5 17.5T720-820q0 6-1.5 12t-3.5 11q-4 11-7.5 22.5T702-751l91 91h87v279l-113 37-67 224H480v-80h-80v80H180Zm60-80h80v-80h240v80h80l62-206 98-33v-141h-40L620-720q0-20 2.5-38.5T630-796q-29 8-51 27.5T547-720H300q-58 0-99 41t-41 99q0 98 27 191.5T240-200Zm240-298Z"/></svg>
            </div>
            <div style={{display: toastVisible ? 'flex' : 'none'}} className={`${positionStyles} bg-white flex-col w-[150px] p-[5px] shadow-lg border border-black text-[12px] z-[1]`}>
                {cardToastMessage}
            </div>
        </div>
        
    )
}