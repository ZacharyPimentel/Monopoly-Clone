import { ActionButtons } from "../ActionButtons"
import { usePlayer } from "../../../hooks/usePlayer"
import { useGameState } from "../../../stateProviders/GameStateProvider"
import { Player } from "../../../types/controllers/Player"
import {useForm, FormProvider} from 'react-hook-form'
import React from "react"
import { PlayerOfferView } from "./components/PlayerOfferView"
import { useWebSocket } from "../../../hooks/useWebSocket"

type CreateTradeInputs = {
    playerOne:{
        playerId:string,
        money:number,
        getOutOfJailFreeCards:number
        gamePropertyIds: number[]
    },
    playerTwo:{
        playerId:string
        money:number,
        getOutOfJailFreeCards:number
        gamePropertyIds: number[]
    }
}

export const CreateTradeModal:React.FC<{tradeWithPlayer:Player}> = ({tradeWithPlayer}) => {

    const gameState = useGameState()
    const {player} = usePlayer()
    const {invoke} = useWebSocket();

    const form = useForm<CreateTradeInputs>({
        mode:'onBlur',
        defaultValues:{
            playerOne:{
                playerId:player.id,
                money:0,
                getOutOfJailFreeCards:0,
                gamePropertyIds:[]
            },
            playerTwo:{
                playerId:tradeWithPlayer.id,
                money:0,
                getOutOfJailFreeCards:0,
                gamePropertyIds:[]
            },
        }
    })

    console.log(form.formState.isValid)

    return (
        <FormProvider {...form}>
            <div className='flex flex-col gap-[20px]'>
                <p className='font-bold'>Create a Trade</p>
                
                <div className='grid grid-cols-2'>
                    {/* Initiating Trade Player (The Current Client Player) */}
                    <div className='border-r border-black pr-[10px] '>
                        <PlayerOfferView formControlPrefix="playerOne" player={player}/>
                    </div>
                    {/* Other Trade Player */}
                    <div className='border-l border-black pl-[10px] '>
                        <PlayerOfferView formControlPrefix="playerTwo" player={tradeWithPlayer}/>
                    </div>
                </div>
                <ActionButtons
                    confirmButtonStyle="success"
                    confirmCallback={async() => {
                        invoke.trade.create(
                            gameState.gameId,
                            form.getValues('playerOne'),
                            form.getValues('playerTwo')
                        )
                    }}
                    confirmButtonText="Create Trade"
                    confirmDisabled={!form.formState.isValid}
                />
            </div>
        </FormProvider>
    )
}