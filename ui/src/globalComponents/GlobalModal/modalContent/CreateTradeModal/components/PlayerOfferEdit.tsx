import { useMemo } from "react";
import { BoardSpace, Player, Property } from "@generated"
import React from "react";
import { MultiSelect, MultiSelectData, NumberInput } from "@globalComponents";
import { useGameState } from "@stateProviders";

export const PlayerOfferEdit:React.FC<{formControlPrefix:string, player:Player}> = ({formControlPrefix,player}) => {
    
    const gameState = useGameState(['boardSpaces']);

    const multiSelectData:MultiSelectData[] = useMemo( () => {
            return gameState.boardSpaces
                .filter((space): space is BoardSpace & { property: Property  } => 
                    space.property?.playerId === player.id)
                .sort( space => space.id) // want them to appear in board order in the trade window
                .map( (space) => {
                    return {
                        id:space.property.id,
                        display:space.boardSpaceName,
                        value: space.property.gamePropertyId as number,
                        color: space.property.color  ?? 'white'
                    }
                })
        },[player])
    
    return (
        <div className='flex flex-col gap-[20px]' >
            <MultiSelect formControlPrefix={`${formControlPrefix}.gamePropertyIds`} data={multiSelectData}/>
            <div className='mt-auto'>
                <label className='flex flex-col'>
                    <p className=''>Money (${player.money})</p>
                    <NumberInput 
                        formControl={`${formControlPrefix}.money`}
                        min={0}
                        max={player.money < 0 ? 0 : player.money}
                    />
                </label>
                <label className='flex flex-col'>
                    <p className=''>Get Out Of Jail Free Cards ({player.getOutOfJailFreeCards})</p>
                    <NumberInput 
                        formControl={`${formControlPrefix}.getOutOfJailFreeCards`}
                        min={0}
                        max={player.getOutOfJailFreeCards}
                        disabled={player.getOutOfJailFreeCards === 0}
                    />
                </label>
            </div>
        </div>
    )
}