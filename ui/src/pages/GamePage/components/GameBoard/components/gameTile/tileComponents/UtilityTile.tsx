import { BoardSpace } from "@generated"
import { UtilityPopover } from "../components/UtilityPopover";
import { useLayoutEffect, useMemo, useRef } from "react";
import { useGameState } from "@stateProviders";
import useWindowSize from "src/hooks/useWindowSize";

export const UtilityTile:React.FC<{space:BoardSpace,sideClass:string}> = ({space,sideClass}) => {
    const gameState = useGameState(['players']);
    
    const truncateWrapperDiv = useRef<HTMLDivElement>(null)
    const {recalculate} = useWindowSize();

    useLayoutEffect( () => {
        recalculate();
    },[]) 

    if(!space.property)return null

    const property = space.property;

    const propertyStyles = useMemo( () => {
        if(property.boardSpaceId === 13) return({
            position: 'right-[70%] absolute top-[50%] top-[50%] translate-y-[-50%]'
        })
        if(property.boardSpaceId === 29) return({
            position: 'absolute bottom-[100%] left-[50%] translate-x-[-50%]'
        })
        return {position:''};
    },[property])

    return (
        <div className='h-full relative'>
            <div className={`${sideClass} w-full h-full bg-[yellow] text-center flex items-center justify-between shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden`}>
                {property.playerId
                    ? <img className='w-3 h-3 md:w-7 md:h-7 opacity-[0.7]' src={gameState.players.find( (player) => player.id === property.playerId)?.iconUrl}/>
                    : sideClass === 'tile-right' || sideClass === 'tile-left' 
                        ? <p className='bg-[#eaeaea] text-[6px] md:text-[12px] h-full md:w-fit rounded leading-tight md:leading-5'>${property.purchasePrice}</p>
                        : <p className='text-center bg-[#eaeaea] text-[6px] md:text-[12px]  w-full md:w-fit rounded leading-tight md:leading-5'>${property.purchasePrice}</p>

                }
                <div ref={truncateWrapperDiv} className='w-full h-full relative flex items-center justify-center'>
                    {sideClass === 'tile-right' || sideClass === 'tile-left' 
                        ? 
                            <p style={{height:truncateWrapperDiv?.current?.offsetHeight || 0}} className='text-[6px] md:text-[10px] leading-tight truncate lg:overflow-visible lg:whitespace-normal lg:text-center'>
                                {space.boardSpaceName}
                            </p>
                        :
                            <p className='text-[6px] md:text-[10px] leading-tight truncate lg:overflow-visible lg:whitespace-normal lg:text-center'>
                                {space.boardSpaceName}
                            </p>
                    }
                </div>
            </div>
            <UtilityPopover property={property} sideClass={sideClass} space={space} propertyStyles={propertyStyles}/>
        </div>
    )
}