import { useMemo } from "react";
import { Popover } from "../components/Popover";
import { useGameState } from "@stateProviders";

export const PropertyTile:React.FC<{position:number,sideClass:string}> = ({position,sideClass}) => {

    const gameState = useGameState(['boardSpaces','players']);
    const property = gameState.boardSpaces[position - 1]?.property;

    //return the same content but in a different orientation depending on the property set
    if(!property) return null

    const propertyStyles:{position:string,flexDirection:'column'|'row'} = useMemo( () => {
        if(property.setNumber === 1) return({
            position: 'absolute top-[70%] left-[50%] translate-x-[-50%]',
            flexDirection: 'row'
        })
        if(property.setNumber === 2) return({
            position: 'absolute top-[100%] left-[50%] translate-x-[-50%]',
            flexDirection: 'row'
        })
        if(property.setNumber === 3) return({
            position: 'right-[70%] absolute top-[50%] top-[50%] translate-y-[-50%]',
            flexDirection: 'column'
        })
        if(property.setNumber === 4) return({
            position: 'right-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]',
            flexDirection: 'column'
        })
        if(property.setNumber === 5) return({
            position: 'absolute bottom-[100%] left-[50%] translate-x-[-50%]',
            flexDirection: 'row'
        })
        if(property.setNumber === 6) return({
            position: 'absolute bottom-[100%] left-[50%] translate-x-[-50%]',
            flexDirection: 'row'
        })
        if(property.setNumber === 7) return({
            position: 'left-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]',
            flexDirection: 'column'
        })
        if(property.setNumber === 8) return({
            position: 'left-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]',
            flexDirection: 'column'
        })
        return {position:'', flexDirection: 'row'};
    },[property])

    return (
        <div className='h-full relative'>
            {/* The tile */}
            <div className={`${sideClass} w-full h-full bg-white flex items-center text-center justify-between shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden relative`}>
                {property.mortgaged && 
                    <div className='absolute bg-black inset-0 opacity-[0.5]'></div>
                }
                {property.playerId
                    ? <img className='w-[30px] h-[30px] opacity-[0.7]' src={gameState.players.find( (player) => player.id === property.playerId)?.iconUrl}/>
                    : <p className='text-center bg-[#eaeaea]'>${property.purchasePrice}</p>

                }
                <p className='p-[5px] text-[12px] leading-tight'>
                    {gameState.boardSpaces[position - 1].boardSpaceName}
                </p>
                <span style={{backgroundColor:property.color, flexDirection: propertyStyles.flexDirection}} className='flex justify-center relative items-center px-[5px] gap-[5px]'>
                    {Array.from({length:property.upgradeCount}).map( () => {
                        return (
                            <span className='w-[10px] h-[10px] rounded-[50%] bg-black'></span>
                        )
                    })}
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