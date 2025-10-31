import { useLayoutEffect, useMemo, useRef } from "react";
import { Popover } from "../components/Popover";
import { useGameState } from "@stateProviders";
import useWindowSize from "src/hooks/useWindowSize";
import { fixColorContrast } from "@helpers";

export const PropertyTile:React.FC<{position:number,sideClass:string}> = ({position,sideClass}) => {

    const gameState = useGameState(['boardSpaces','players']);
    const property = gameState.boardSpaces[position - 1]?.property;

    const truncateWrapperDiv = useRef<HTMLDivElement>(null)
    const {recalculate} = useWindowSize();

    useLayoutEffect( () => {
        recalculate();
    },[]) 

    //return the same content but in a different orientation depending on the property set
    if(!property) return null

    const propertyStyles:{position:string,flexDirection:'column'|'row'} = useMemo( () => {
        if(property.setNumber === 1) return({
            position: 'absolute top-[100%] left-0',
            flexDirection: 'row'
        })
        if(property.setNumber === 2) return({
            position: 'absolute top-[100%] right-0',
            flexDirection: 'row'
        })
        if(property.setNumber === 3) return({
            position: 'right-[100%] absolute top-[0]',
            flexDirection: 'column'
        })
        if(property.setNumber === 4) return({
            position: 'right-[100%] absolute bottom-0',
            flexDirection: 'column'
        })
        if(property.setNumber === 5) return({
            position: 'absolute bottom-full right-0',
            flexDirection: 'row'
        })
        if(property.setNumber === 6) return({
            position: 'absolute bottom-full left-0',
            flexDirection: 'row'
        })
        if(property.setNumber === 7) return({
            position: 'left-[100%] absolute bottom-0',
            flexDirection: 'column'
        })
        if(property.setNumber === 8) return({
            position: 'left-[100%] absolute top-0',
            flexDirection: 'column'
        })
        return {position:'', flexDirection: 'row'};
    },[property])

    return (
        <div className='h-full relative'>
            {/* The tile */}
            <div className={`${sideClass} w-full h-full bg-white flex items-center text-center justify-between shadow-lg border border-totorodarkgreen rounded-[5px] relative`}>
                {property.mortgaged && 
                    <div className='absolute bg-black inset-0 opacity-[0.5]'></div>
                }
                {property.playerId
                    ? <img className='w-3 h-3 md:w-7 md:h-7 opacity-[0.7]' src={gameState.players.find( (player) => player.id === property.playerId)?.iconUrl}/>
                        : sideClass === 'tile-right' || sideClass === 'tile-left' 
                            ? <p className='bg-[#eaeaea] text-[6px] md:text-[12px] md:min-w-[20px] h-full md:w-fit rounded leading-tight md:leading-5'>${property.purchasePrice}</p>
                            : <p className='text-center bg-[#eaeaea] text-[6px] md:text-[12px] w-full rounded leading-tight md:leading-5'>${property.purchasePrice}</p>

                }
                <div ref={truncateWrapperDiv} className='w-full h-full relative flex items-center justify-center'>
                    {sideClass === 'tile-right' || sideClass === 'tile-left' 
                        ? 
                            <p style={{height:truncateWrapperDiv?.current?.offsetHeight || 0}} className='text-[6px] md:text-[10px] leading-tight truncate lg:overflow-visible lg:whitespace-normal'>
                                {gameState.boardSpaces[position - 1].boardSpaceName}
                            </p>
                        :
                            <p className='text-[6px] md:text-[10px] leading-tight truncate lg:overflow-visible lg:whitespace-normal'>
                                {gameState.boardSpaces[position - 1].boardSpaceName}
                            </p>
                    }
                </div>
                <span style={{backgroundColor:property.color, flexDirection: propertyStyles.flexDirection}} className='flex justify-center relative items-center px-[5px] gap-[5px]'>
                    {Array.from({length:property.upgradeCount}).map( (_,index) => {
                        return (
                            <span key={index} className='w-[10px] h-[10px] rounded-[50%] bg-black hidden lg:flex'></span>
                        )
                    })}
                    <span style={{
                        color:fixColorContrast('white',property!.color),
                        scale:property.setNumber === 3 || property.setNumber === 4 ? -1 : 1
                    }} className='w-full lg:hidden text-[6px] md:text-[12px] font-bold'>
                        {property.upgradeCount > 0 && property.upgradeCount}
                    </span>
                </span>
            </div>
            <Popover 
                sideClass={sideClass}
                space={gameState.boardSpaces[position - 1]}
                property={property}
                propertyStyles={propertyStyles}
            />
        </div>
        
    )
}