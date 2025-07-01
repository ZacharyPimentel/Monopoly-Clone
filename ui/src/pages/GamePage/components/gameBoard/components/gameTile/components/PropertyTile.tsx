import { useMemo } from "react";
import { useGameState } from "@stateProviders/GameStateProvider";

export const PropertyTile:React.FC<{position:number,sideClass:string}> = ({position,sideClass}) => {

    const gameState = useGameState();
    const property = gameState.boardSpaces[position - 1]?.property;

    //return the same content but in a different orientation depending on the property set
    if(!property) return null

    const propertyStyles = useMemo( () => {
        if(property.setNumber === 1) return({
            position: 'absolute top-[100%] left-[50%] translate-x-[-50%]'
        })
        if(property.setNumber === 2) return({
            position: 'absolute top-[100%] left-[50%] translate-x-[-50%]'
        })
        if(property.setNumber === 3) return({
            position: 'right-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]'
        })
        if(property.setNumber === 4) return({
            position: 'right-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]'
        })
        if(property.setNumber === 5) return({
            position: 'absolute bottom-[100%] left-[50%] translate-x-[-50%]'
        })
        if(property.setNumber === 6) return({
            position: 'absolute bottom-[100%] left-[50%] translate-x-[-50%]'
        })
        if(property.setNumber === 7) return({
            position: 'left-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]'
        })
        if(property.setNumber === 8) return({
            position: 'left-[100%] absolute top-[50%] top-[50%] translate-y-[-50%]'
        })
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
                <span style={{backgroundColor:property.color}} className='flex'></span>
            </div>
            {/* The popover */}
            <div className={`${propertyStyles?.position} bg-white hidden group-hover:flex flex-col w-[150px] p-[5px] shadow-lg border border-black text-[12px]`}>
                <p className='mb-[5px]'>{gameState.boardSpaces[position - 1].boardSpaceName}</p>
                <div className='flex justify-between border-b border-black'>
                    <p>Upgrades</p>
                    <p>Rent</p>
                </div>
                {property.propertyRents.map( (propertyRent,index) => {
                    return (
                        <div 
                            key={propertyRent.id}
                            style={{fontWeight:property.upgradeCount === index ? 'bold' : 'normal'}} 
                            className='flex justify-between'
                        >
                            <p>{propertyRent.upgradeNumber}</p>
                            <p>${propertyRent.rent}</p>
                        </div>
                    )
                })}
                <div className='border-t border-black'>
                    <p>House Cost: ${property.upgradeCost}</p>
                    <p>Mortgage Value: ${property.mortgageValue}</p>
                </div>
            </div>
        </div>
        
    )
}