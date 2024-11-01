import { useRef } from "react";
import { GameInformation } from "./components/gameInformation/GameInformation"
import { GameTile } from "./components/gameTile/GameTile";
import TotoroFamily from './assets/totorofamily.svg?react';
import { CenterBoardDisplay } from "./components/CenterBoardDisplay/CenterBoardDisplay";

export const GameBoard = () => {

    const gameBoardRef = useRef<HTMLDivElement | null>(null);
    return (
        <div className='justify-center w-full h-full flex flex-wrap'>
            <div ref={gameBoardRef} className='p-[10px] overflow-hidden rotate-[0deg] w-[100vmin] md:h-full md:max-h-[100vh] aspect-square bg-totorodarkgreen relative'>
                <CenterBoardDisplay/>
                <div className='flex w-full'>
                    {/* Top Left Square */}
                    <div className='aspect-square min-w-[10%]'>
                        <GameTile position={1} />
                    </div>

                    {/* Top Row */}
                    <div className='flex-1 flex'>
                        {Array.from(new Array(9)).map( (_,index:number) => {
                            return(
                                /* position = the zero index plus one, plus the amount of positions prior */
                                <div key={index} className='flex-1'><GameTile position={2+index} /></div>
                            )
                        })}

                    </div>
                    {/* Top Right Square */}
                    <div className='aspect-square min-w-[10%]'>
                        <GameTile position={11} />
                    </div>
                </div>

                {/* Left Row */}
                <div className='flex flex-1'>
                    <div className='flex min-w-[10%] flex-col-reverse'>
                        {Array.from(new Array(9)).map( (_,index:number) => {
                            return(
                                /* position = the zero index plus one, plus the amount of positions prior */
                                <div key={index} className='flex-1'><GameTile position={32+index} /></div>
                            )
                        })}
                    </div>
                    {/* Big Center Square */}
                    <div className='flex-1 aspect-square relative'>
                        <div className='absolute w-full h-full flex items-center justify-center opacity-[0.3]'>
                            <TotoroFamily/>
                        </div>
                        
                    </div>

                    {/* Right Row */}
                    <div className='flex flex-col min-w-[10%]'>
                        {Array.from(new Array(9)).map( (_,index:number) => {
                            return(
                                /* position = the zero index plus one, plus the amount of positions prior */
                                <div key={index} className='flex-1'><GameTile position={12+index} /></div>
                            )
                        })}
                    </div>
                </div>
                
                <div className='flex w-full'>
                    {/* Bottom Left Square */}
                    <div className='aspect-square min-w-[10%]'>
                        <GameTile position={31} />
                    </div>

                    {/* Bottom Row */}
                    <div className='flex-1 flex flex-row-reverse'>
                        {Array.from(new Array(9)).map( (_,index:number) => {
                            return(
                                /* position = the zero index plus one, plus the amount of positions prior */
                                <div key={index} className='flex-1'><GameTile position={22+index} /></div>
                            )
                        })}
                    </div>
                    {/* Bottom Right Square */}
                    <div className='aspect-square min-w-[10%]'>
                        <GameTile position={21} />
                    </div>
                </div>
            </div>
            <div className='h-[100vh] min-w-[300px] flex-1 relative overflow-y-scroll'>
                <GameInformation />
            </div>
        </div>
    )
}