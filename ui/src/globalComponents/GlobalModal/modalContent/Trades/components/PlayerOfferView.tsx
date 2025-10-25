import { useMemo } from "react";
import { Player } from "@generated"
import React from "react";
import { NumberInput } from "@globalComponents";
import { useGameState } from "@stateProviders";
import { useFormContext } from "react-hook-form";
import { fixColorContrast } from "@helpers";

export const PlayerOfferView:React.FC<{formControlPrefix:string, player:Player}> = ({formControlPrefix,player}) => {
    

    const form = useFormContext();
    const {boardSpaces} = useGameState(['boardSpaces'])

    const tradeBoardSpaces = boardSpaces.filter( (bs) => form.getValues(`${formControlPrefix}.gamePropertyIds`).includes(bs?.property?.id))



    return (
        <div className='flex flex-col gap-[20px] h-full' >
            <div className='flex flex-col gap-[10px] mb-auto'>
                {tradeBoardSpaces.length > 0 && <>
                    <div className='h-[150px] overflow-y-scroll flex flex-col bg-white cursor-not-allowed relative'>
                        {tradeBoardSpaces.map( (space) => {
                            const color = space?.property?.color ?? 'white'
                            return (
                                <React.Fragment key={space.id}>
                                    <div className='p-[5px] relative'>
                                        <p className='relative z-[1] flex items-center gap-[5px]' style={{color:fixColorContrast(color,color)}}>
                                            {space.boardSpaceName}
                                            <span style={{backgroundColor:color}} className='w-[15px] h-[15px] rounded-full'></span>
                                        </p>
                                        <span style={{opacity:0.5,backgroundColor:color}} className='absolute inset-0'></span>
                                    </div>
                                </React.Fragment>)
                        })}
                    </div>
                </>}
                {tradeBoardSpaces.length === 0 && <p className='opacity-[0.5]'><i>No properties to trade</i></p>}
            </div>
            <label className='flex flex-col'>
                <p className=''>Money (${player.money})</p>
                <NumberInput 
                    formControl={`${formControlPrefix}.money`}
                    min={0}
                    max={player.money < 0 ? 0 : player.money}
                    disabled={true}
                />
            </label>
            <label className='flex flex-col'>
                <p className=''>Get Out Of Jail Free Cards ({player.getOutOfJailFreeCards})</p>
                <NumberInput 
                    formControl={`${formControlPrefix}.getOutOfJailFreeCards`}
                    min={0}
                    max={player.getOutOfJailFreeCards}
                    disabled={true}
                />
            </label>
        </div>
    )
}