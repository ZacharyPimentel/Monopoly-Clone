import { useMemo } from "react";
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider";

export const PropertyTile:React.FC<{position:number,sideClass:string}> = ({position,sideClass}) => {

    const gameState = useGameState();
    const property = gameState.boardSpaces[position - 1]?.property;

    //return the same content but in a different orientation depending on the property set
    if(!property) return null

    const propertyColor = useMemo( () => {
        if(property.setNumber === 1) return 'bg-monopolyBrown'
        if(property.setNumber === 2) return 'bg-monopolyLightBlue'
        if(property.setNumber === 3) return 'bg-monopolyPink'
        if(property.setNumber === 4) return 'bg-monopolyOrange'
        if(property.setNumber === 5) return 'bg-monopolyRed'
        if(property.setNumber === 6) return 'bg-monopolyYellow'
        if(property.setNumber === 7) return 'bg-monopolyGreen'
        if(property.setNumber === 8) return 'bg-monopolyBlue'
    },[property])

    return (
        <button className={`${sideClass} w-full h-full bg-white flex items-center justify-between shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden`}>
            {property.playerId
                ? <img className='w-[30px] h-[30px] opacity-[0.7]' src={gameState.players.find( (player) => player.id === property.playerId)?.iconUrl}/>
                : <p className='text-center bg-[#eaeaea]'>${property.purchasePrice}</p>

            }
            <p className='absolute top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%]'>Property</p>
            <span className={`flex ${propertyColor}`}></span>
        </button>
    )
}