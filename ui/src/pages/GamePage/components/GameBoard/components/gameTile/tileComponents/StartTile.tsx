import { BoardSpace } from "@generated"

export const StartTile:React.FC<{space:BoardSpace}> = ({space}) => {
    return (
        <div className='flex flex-col items-center justify-center h-full shadow-lg border bg-totorolightgreen border-totorodarkgreen rounded-[5px]'>
            <p className='font-totoro md:text-xl'>{space.boardSpaceName}</p>
            <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="M647-440H160v-80h487L423-744l57-56 320 320-320 320-57-56 224-224Z"/></svg>
        </div>
    )
}