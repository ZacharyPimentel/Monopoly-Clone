import { BoardSpace } from "@generated"
import { RailroadPopover } from "../components/RailroadPopover";
import { useMemo } from "react";
import { useGameState } from "@stateProviders";

export const Railroad:React.FC<{space:BoardSpace,sideClass:string}> = ({space,sideClass}) => {

    const gameState = useGameState(['players']);

    if(!space.property)return null

    const property = space.property;

    const propertyStyles = useMemo( () => {
            if(property.boardSpaceId === 6) return({
                position: 'absolute top-[70%] left-[50%] translate-x-[-50%]'
            })
            if(property.boardSpaceId === 16) return({
                position: 'right-[70%] absolute top-[50%] top-[50%] translate-y-[-50%]'
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
            <div className={`${sideClass} w-full h-full bg-totorogrey flex items-center text-center justify-between shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden relative`}>
                {property.mortgaged && 
                    <div className='absolute bg-black inset-0 opacity-[0.5]'></div>
                }
                {property.playerId
                    ? <img className='w-[30px] h-[30px] opacity-[0.7]' src={gameState.players.find( (player) => player.id === property.playerId)?.iconUrl}/>
                    : <p className='text-center bg-[#eaeaea]'>${property.purchasePrice}</p>

                }
                <p className='p-[5px] text-[12px] leading-tight'>
                    {space.boardSpaceName}
                </p>
                <span style={{backgroundColor:property.color}} className='flex'></span>
            </div>
            <RailroadPopover property={property} space={space} sideClass={sideClass} propertyStyles={propertyStyles}/>
        </div>
    )
}