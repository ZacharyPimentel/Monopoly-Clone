import { useMemo } from "react"
import { ActionButtons } from "../../../../../../../globalComponents/GlobalModal/ActionButtons"
import { usePlayer } from "../../../../../../../hooks/usePlayer"
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider"
import { useApi } from "../../../../../../../hooks/useApi"
import { Player } from "../../../../../../../types/controllers/Player"
import {useForm, FormProvider, useFormContext} from 'react-hook-form'
import { NumberInput } from "../../../../../../../globalComponents/FormElements/NumberInput"
import { TradePropertyItem } from "../components/TradePropertyItem"

type CreateTradeInputs = {
    playerOne:{
        money:number,
        getOutOfJailFreeCards:number
        gamePropertyIds: number[]
    },
    playerTwo:{
        money:number,
        getOutOfJailFreeCards:number
        gamePropertyIds: number[]
    }
}

export const CreateTradeModal:React.FC<{tradeWithPlayer:Player}> = ({tradeWithPlayer}) => {

    const gameState = useGameState()
    const {player} = usePlayer()
    const api = useApi();

    const tradeWithPlayerProperties = useMemo( () => {
        return gameState.boardSpaces.filter( (space) => space.property && space.property?.playerId === tradeWithPlayer.id);
    },[tradeWithPlayer])

    const playerProperties = useMemo( () => {
        return gameState.boardSpaces.filter( (space) => space.property && space.property?.playerId === player.id);
    },[tradeWithPlayer])

    const form = useForm<CreateTradeInputs>({
        mode:'onBlur',
        defaultValues:{
            playerOne:{
                money:0,
                getOutOfJailFreeCards:0
            },
            playerTwo:{
                money:0,
                getOutOfJailFreeCards:0
            },
        }
    })

    return (
        <FormProvider {...form}>
            <div className='flex flex-col gap-[20px]'>
                <p className='font-bold'>Create a Trade</p>
                
                <div className='grid grid-cols-2'>
                    {/* Initiating Trade Player (The Current Client Player) */}
                    <div className='border-r border-black pr-[10px] flex flex-col gap-[20px]' >
                        <div className='flex items-center gap-[20px]'>
                            <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                            <p>{player.playerName}</p>
                        </div>
                        <div className='flex flex-col gap-[10px]'>
                            <p>Properties:</p>
                            {playerProperties.length > 0 && <>
                                <div className='h-[150px] overflow-y-scroll'>
                                    
                                    {playerProperties.map( (space) => {
                                        return (
                                            <TradePropertyItem space={space}/>
                                        )
                                    })}
                                </div>
                            </>}
                            {playerProperties.length === 0 && <p className='opacity-[0.5]'><i>No properties to trade</i></p>}
                        </div>
                        <label className='flex flex-col'>
                            <p className=''>Money (${player.money})</p>
                            <NumberInput 
                                formControl='playerOne.money'
                                min={0}
                                max={player.money}
                            />
                        </label>
                        <label className='flex flex-col'>
                            <p className=''>Get Out Of Jail Free Cards ({player.getOutOfJailFreeCards})</p>
                            <NumberInput 
                                formControl='playerOne.getOutOfJailFreeCards'
                                min={0}
                                max={player.getOutOfJailFreeCards}
                                disabled={player.getOutOfJailFreeCards === 0}
                            />
                        </label>
                    </div>
                    {/* Other Trade Player */}
                    <div className='border-l border-black pl-[10px] flex flex-col gap-[20px]'>
                        <div className='flex items-center gap-[20px] justify-end'>
                            <img className='w-[30px] h-[30px]' src={tradeWithPlayer.iconUrl}/>
                            <p>{tradeWithPlayer.playerName}</p>
                        </div>
                        <div className='flex flex-col'>
                            <p>Properties:</p>
                            {tradeWithPlayerProperties.length > 0 && tradeWithPlayerProperties.map( (space) => {
                                return (
                                    <div>{space.boardSpaceName}</div>
                                )
                            })}
                            {tradeWithPlayerProperties.length === 0 && <p className='opacity-[0.5]'><i>No properties to trade</i></p>}
                        </div>
                        <label className='flex flex-col'>
                            <p className=''>Money (${tradeWithPlayer.money})</p>
                            <NumberInput 
                                formControl='playerTwo.money'
                                min={0}
                                max={tradeWithPlayer.money}
                            />
                        </label>
                        <label className='flex flex-col'>
                            <p className=''>Get Out Of Jail Free Cards ({tradeWithPlayer.getOutOfJailFreeCards})</p>
                            <NumberInput 
                                formControl='playerTwo.getOutOfJailFreeCards'
                                min={0}
                                max={tradeWithPlayer.getOutOfJailFreeCards}
                                disabled={tradeWithPlayer.getOutOfJailFreeCards === 0}
                            />
                        </label>
                    </div>
                </div>
                <ActionButtons
                    confirmButtonStyle="success"
                    confirmCallback={async() => {}}
                    confirmButtonText="Create Trade"
                    confirmDisabled={form.formState.isValid}
                />
            </div>
        </FormProvider>
    )
}