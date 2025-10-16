import { useGameState } from "@stateProviders/GameStateProvider";
import { BoardSpace } from "@generated/index"
import { UtilityPopover } from "../components/UtilityPopover";
import { useMemo } from "react";

export const UtilityTile:React.FC<{space:BoardSpace,sideClass:string}> = ({space,sideClass}) => {
    const gameState = useGameState();

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
            <div className={`${sideClass} w-full h-full bg-[yellow] flex items-center justify-between shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden`}>
                {property.playerId
                    ? <img className='w-[30px] h-[30px] opacity-[0.7]' src={gameState.players.find( (player) => player.id === property.playerId)?.iconUrl}/>
                    : <p className='text-center bg-[#eaeaea]'>${property.purchasePrice}</p>

                }
                <p className='absolute top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%] text-[12px]'>
                    {space.boardSpaceName}
                </p>
            </div>
            <UtilityPopover property={property} sideClass={sideClass} space={space} propertyStyles={propertyStyles}/>
        </div>
    )
}