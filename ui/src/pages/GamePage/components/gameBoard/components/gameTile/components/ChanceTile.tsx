import { useEffect, useMemo, useState } from "react";
import { useGameDispatch, useGameState } from "../../../../../../../stateProviders/GameStateProvider"
import { BoardSpace } from "../../../../../../../types/controllers/BoardSpace"
import { usePlayer } from "../../../../../../../hooks/usePlayer";

export const ChanceTile:React.FC<{space:BoardSpace}> = ({space}) => {

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
            case 8:{
                return 'absolute top-[100%] left-[50%] translate-x-[-50%]'
            }
            case 23:{
                return 'absolute bottom-[100%] left-[50%] translate-x-[-50%]'
            }
            case 37:{
                return 'left-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]'
            }
        }
    },[space])

    return (
        <div className='h-full relative'>
            <div className='bg-[green] flex items-center justify-center w-full h-full shadow-lg border border-totorodarkgreen rounded-[5px]'>
                <p className='text-[12px]'>{space.boardSpaceName}</p>
            </div>
            <div style={{display: toastVisible ? 'flex' : 'none'}} className={`${positionStyles} bg-white flex-col w-[150px] p-[5px] shadow-lg border border-black text-[12px]`}>
                {cardToastMessage}
            </div>
        </div>
    )
}