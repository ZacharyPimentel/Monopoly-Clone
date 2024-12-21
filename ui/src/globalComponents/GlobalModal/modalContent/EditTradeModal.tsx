import { ActionButtons } from "../ActionButtons"
import { usePlayer } from "../../../hooks/usePlayer"
import { useGameState } from "../../../stateProviders/GameStateProvider"
import { Player } from "../../../types/controllers/Player"
import {useForm, FormProvider} from 'react-hook-form'
import React, { useMemo } from "react"
import { PlayerOfferView } from "./components/PlayerOfferView"
import { useWebSocket } from "../../../hooks/useWebSocket"
import { Trade } from "../../../types/websocket/Trade"

type EditTradeInputs = {
    playerOne:{
        playerId:string,
        initiator:boolean
        money:number,
        getOutOfJailFreeCards:number
        gamePropertyIds: number[]
    },
    playerTwo:{
        playerId:string
        initiator:boolean
        money:number,
        getOutOfJailFreeCards:number
        gamePropertyIds: number[]
    }
}

export const EditTradeModal:React.FC<{trade:Trade}> = ({trade}) => {

    const gameState = useGameState()
    const {player} = usePlayer()
    const {invoke} = useWebSocket();

    const currentPlayerOffer = useMemo( () => {
        for(let i=0 ; i<trade.playerTrades.length ; i++){
            if(trade.playerTrades[i].playerId === player.id){
                return trade.playerTrades[i]
            }
        }
    },[])

    const otherPlayerOffer = useMemo( () => {
        for(let i=0 ; i<trade.playerTrades.length ; i++){
            if(trade.playerTrades[i].playerId !== player.id){
                return trade.playerTrades[i]
            }
        }
    },[])

    const otherPlayer = gameState.players.find( x => x.id === otherPlayerOffer?.playerId)

    const form = useForm<EditTradeInputs>({
        mode:'onBlur',
        defaultValues:{
            playerOne:{
                ...currentPlayerOffer,
                gamePropertyIds: currentPlayerOffer?.tradeProperties.map( property => property.gamePropertyId)
            },
            playerTwo:{
                ...otherPlayerOffer,
                gamePropertyIds: otherPlayerOffer?.tradeProperties.map( property => property.gamePropertyId)
            }
        }
    })

    if(!otherPlayer)return

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
                        <PlayerOfferView formControlPrefix="playerTwo" player={otherPlayer}/>
                    </div>
                </div>
                <ActionButtons
                    confirmButtonStyle="success"
                    confirmCallback={async() => {
                        //the player that initiated the trade last cannot accept the trade, only modify it
                        if(currentPlayerOffer?.initiator){
                            invoke.trade.update(
                                trade.id,
                                form.getValues('playerOne'),
                                form.getValues('playerTwo')
                            )
                            return
                        }
                        //the player that didn't initate the trade can counter trade, or accept trade
                        //TO DO
                    }}
                    confirmButtonText={currentPlayerOffer?.initiator ? 'Modify Trade' : "Accept Trade"}
                    confirmDisabled={!form.formState.isValid}
                />
            </div>
        </FormProvider>
    )
}