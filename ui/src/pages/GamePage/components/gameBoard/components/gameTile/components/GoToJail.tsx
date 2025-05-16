/// <reference types="vite-plugin-svgr/client" />
import { BoardSpace } from '../../../../../../../types/controllers/BoardSpace';
import CatBus from '../assets/catbus.svg?react';

export const GoToJail:React.FC<{space:BoardSpace}> = ({space}) => {
    return (
        <div className='flex flex-col items-center justify-start h-full shadow-lg border border-totorodarkgreen'>
            <p className='text-[1.4rem] font-totoro'>{space.boardSpaceName}</p>
            <div className='w-[80%] absolute bottom-0'>
                <CatBus/>
            </div>
        </div>
    )
}