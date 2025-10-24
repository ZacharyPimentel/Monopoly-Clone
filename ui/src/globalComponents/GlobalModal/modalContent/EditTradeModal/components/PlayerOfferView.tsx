import { useMemo } from "react";
import { Player } from "@generated"
import React from "react";
import { TradePropertyItem } from "./TradePropertyItem";
import { NumberInput } from "@globalComponents";
import { useGameState } from "@stateProviders";

export const PlayerOfferView:React.FC<{formControlPrefix:string, player:Player}> = ({formControlPrefix,player}) => {
    
    const gameState = useGameState(['boardSpaces']);

    const playerProperties = useMemo( () => {
        return gameState.boardSpaces.filter( (space) => space.property && space.property?.playerId === player.id);
    },[player])
    
    return (
        <div className='flex flex-col gap-[20px] h-full' >
            <div className='flex flex-col gap-[10px] mb-auto'>
                {playerProperties.length > 0 && <>
                    <div className='h-[150px] overflow-y-scroll flex flex-col'>
                        {playerProperties.map( (space) => {
                            return (<React.Fragment key={space.id}>
                                <TradePropertyItem formControl={`${formControlPrefix}.gamePropertyIds`} space={space}/>
                            </React.Fragment>)
                        })}
                    </div>
                </>}
                {playerProperties.length === 0 && <p className='opacity-[0.5]'><i>No properties to trade</i></p>}
            </div>
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
    )
}