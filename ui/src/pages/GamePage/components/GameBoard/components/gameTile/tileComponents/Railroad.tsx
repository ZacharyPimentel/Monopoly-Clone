import { BoardSpace } from "@generated"
import { RailroadPopover } from "../components/RailroadPopover";
import { useLayoutEffect, useMemo, useRef } from "react";
import { useGameState } from "@stateProviders";
import useWindowSize from "src/hooks/useWindowSize";

export const Railroad:React.FC<{space:BoardSpace,sideClass:string}> = ({space,sideClass}) => {

    const gameState = useGameState(['players']);

    const truncateWrapperDiv = useRef<HTMLDivElement>(null)
    const {recalculate} = useWindowSize();

    useLayoutEffect( () => {
        recalculate();
    },[]) 

    if(!space.property)return null

    const property = space.property;

    const propertyStyles = useMemo( () => {
            if(property.boardSpaceId === 6) return({
                position: 'absolute top-[100%] left-[50%] translate-x-[-50%]'
            })
            if(property.boardSpaceId === 16) return({
                position: 'right-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]'
            })
            if(property.boardSpaceId === 26) return({
                position: 'absolute bottom-[100%] left-[50%] translate-x-[-50%]'
            })
            if(property.boardSpaceId === 36) return({
                position: 'left-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]'
            })
            return {position:''};
        },[property])

    return (
        <div className='h-full relative'>
            {/* The tile */}
            <div className={`${sideClass} w-full h-full bg-totorogrey flex items-center text-center justify-between shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden relative gap-[2px]`}>
                {property.mortgaged && 
                    <div className='absolute bg-black inset-0 opacity-[0.5]'></div>
                }
                {property.playerId
                    ? <img className='w-3 h-3 md:w-7 md:h-7 opacity-[0.7]' src={gameState.players.find( (player) => player.id === property.playerId)?.iconUrl}/>
                    : sideClass === 'tile-right' || sideClass === 'tile-left' 
                        ? <p className='bg-[#eaeaea] text-[6px] md:text-[12px] h-full md:w-fit rounded leading-tight md:leading-5'>${property.purchasePrice}</p>
                        : <p className='text-center bg-[#eaeaea] text-[6px] md:text-[12px] w-full rounded leading-tight md:leading-5'>${property.purchasePrice}</p>

                }
                <div ref={truncateWrapperDiv} className='w-full h-full relative flex items-center justify-center'>
                    {sideClass === 'tile-right' || sideClass === 'tile-left' 
                        ? 
                            <p style={{height:truncateWrapperDiv?.current?.offsetHeight || 0}} className='text-[6px] md:text-[10px] leading-tight truncate lg:overflow-visible lg:whitespace-normal'>
                                {space.boardSpaceName}
                            </p>
                        :
                            <p className='text-[6px] md:text-[10px] leading-tight truncate lg:overflow-visible lg:whitespace-normal'>
                                {space.boardSpaceName}
                            </p>
                    }
                </div>
            </div>
            <RailroadPopover property={property} space={space} sideClass={sideClass} propertyStyles={propertyStyles}/>
        </div>
    )
}