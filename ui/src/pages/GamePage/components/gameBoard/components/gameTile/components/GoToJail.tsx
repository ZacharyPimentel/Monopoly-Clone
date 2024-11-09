/// <reference types="vite-plugin-svgr/client" />
import CatBus from '../assets/catbus.svg?react';

export const GoToJail = () => {
    return (
        <div className='flex flex-col items-center justify-start h-full bg-totorolightgreen shadow-lg border border-totorodarkgreen rounded-[5px]'>
            <p className='text-[1.4rem] font-totoro'>Go To Jail</p>
            <div className='w-[80%] absolute bottom-0'>
                <CatBus/>
            </div>
        </div>
    )
}