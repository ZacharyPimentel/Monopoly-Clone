/// <reference types="vite-plugin-svgr/client" />
import { BoardSpace } from "@generated"
import CatBus from '../assets/catbus.svg?react';

export const GoToJail:React.FC<{space:BoardSpace}> = ({space}) => {
    return (
        <div className='flex flex-col items-center justify-start h-full bg-totorolightgreen shadow-lg border border-totorodarkgreen rounded-[5px]'>
            <p className='text-[8px] md:text-[14px] font-totoro leading-[1] p-[1px] md:p-[5px] text-center'>{space.boardSpaceName}</p>
            <div className='w-[80%] absolute bottom-0'>
                <CatBus/>
            </div>
        </div>
    )
}