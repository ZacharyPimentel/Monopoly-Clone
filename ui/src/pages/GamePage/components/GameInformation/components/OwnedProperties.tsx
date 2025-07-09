import { useCallback, useMemo } from "react";
import { usePlayer } from "@hooks/usePlayer";
import { useGameState } from "@stateProviders/GameStateProvider"
import { BoardSpace, Property } from "@generated/index";
import { useGlobalContext } from "@stateProviders/GlobalStateProvider";
import { MortgagePropertyModal } from "@globalComponents/GlobalModal/modalContent/MortgagePropertyModal";
import { UnmortgagePropertyModal } from "@globalComponents/GlobalModal/modalContent/UnmortgagePropertyModal";

export const OwnedProperties = () => {

    const gameState = useGameState();
    const {player} = usePlayer();
    const {globalDispatch} = useGlobalContext();

    const propertyColor = useCallback( (property:Property) => {
        if(property.setNumber === 1) return 'bg-monopolyBrown'
        if(property.setNumber === 2) return 'bg-monopolyLightBlue'
        if(property.setNumber === 3) return 'bg-monopolyPink'
        if(property.setNumber === 4) return 'bg-monopolyOrange'
        if(property.setNumber === 5) return 'bg-monopolyRed'
        if(property.setNumber === 6) return 'bg-monopolyYellow'
        if(property.setNumber === 7) return 'bg-monopolyGreen'
        if(property.setNumber === 8) return 'bg-monopolyBlue'
    },[])

    const playerSpaces = gameState.boardSpaces
        .filter(space => space.property)
        .filter(space => space.property?.playerId === player?.id) as BoardSpace[];

    const groupedSets = useMemo( () => {
        const sets:{[key:number]:BoardSpace[]} = {};
        if(!playerSpaces) return sets;
        for(let i=1; i<=8 ; i++){
            const setProperties = playerSpaces.filter( space => space?.property?.setNumber === i);
            if(setProperties.length > 0){
                sets[i] = setProperties;
            }    
        }
        return sets;
    },[playerSpaces])

    return (
        <div className='flex flex-col gap-[10px] p-[30px] bg-totorogreen w-full flex-1'>
            Owned Properties
            <ul className='flex flex-col gap-[10px]'>
                {gameState.boardSpaces
                    .filter( space => space.property)
                    .filter((space) =>space?.property?.playerId === player?.id)
                    .map( (space) => {
                        if(!space.property)return null
                        return (
                            <li key={space.property.id} className='flex items-center gap-[20px]'>
                                <div className={`h-[30px] w-[30px] rounded-[50%] ${propertyColor(space.property)}`}></div>
                                <p>{space.boardSpaceName}</p>
                            </li>
                        )
                    })}
            </ul>
        </div>        
    )
}