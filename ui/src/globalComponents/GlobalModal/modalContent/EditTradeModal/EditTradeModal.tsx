import { usePlayer, useWebSocket } from "@hooks"
import {useForm, FormProvider} from 'react-hook-form'
import React, { useMemo } from "react"
import { PlayerOfferView } from "./components/PlayerOfferView"
import { Trade } from "../../../../types/websocket/Trade"
import { AdvancedActionButtons, AdvancedButtonConfig } from "@globalComponents"
import { useGameState } from "@stateProviders"
import { ArrowLeftRight } from "lucide-react"

type EditTradeInputs = {
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

export const EditTradeModal:React.FC<{trade:Trade}> = ({trade}) => {

    const gameState = useGameState(['players'])
    const {player} = usePlayer()
    const {invoke} = useWebSocket();

    const leftSideOffer = useMemo( () => {
        const tradePlayerIds = trade.playerTrades.map( trade => trade.playerId);
        //if player is part of the trade, should always be listed on the left
        if(tradePlayerIds.includes(player.id)){
            return trade.playerTrades.find(pt => pt.playerId === player.id)
        //otherwise display the player who updated last on the left
        }else{
            return trade.playerTrades.find(pt => pt.playerId === trade.lastUpdatedBy)
        }
    },[])

    const leftSidePlayer = gameState.players.find( p => p.id === leftSideOffer?.playerId)

    const rightSideOffer = useMemo( () => {
        const tradePlayerIds = trade.playerTrades.map( trade => trade.playerId);
        //if player is part of the trade, trade partner is always on the right
        if(tradePlayerIds.includes(player.id)){
            return trade.playerTrades.find(pt => pt.playerId !== player.id)
        //otherwise display the player who didn't update last on the right
        }else{
            return trade.playerTrades.find(pt => pt.playerId !== trade.lastUpdatedBy)
        }
    },[])

    const rightSidePlayer = gameState.players.find( x => x.id === rightSideOffer?.playerId)

    const advancedbuttonConfig:AdvancedButtonConfig[] =  useMemo( () => {
        
        const config:AdvancedButtonConfig[] = [];
        // allow modify trade
        if(player.id !== trade.lastUpdatedBy){
            config.push({
                buttonText:'Modify',
                buttonStyle:'success',
                buttonCallback: async() => {
                    invoke.trade.update({
                        tradeId: trade.id,
                        playerOne:form.getValues().playerOne,
                        playerTwo:form.getValues().playerTwo
                    })
                }
            })
            config.push({
                buttonText:'Accept',
                buttonStyle:'success',
                buttonCallback: async() => {
                    invoke.trade.accept({tradeId:trade.id})
                }
            })
        }
        config.push({
            buttonText:'Decline',
            buttonStyle:'warning',
            buttonCallback: async() => {
                invoke.trade.decline({ tradeId: trade.id });
            }
        })
        return config;
    },[])

    const form = useForm<EditTradeInputs>({
        mode:'onBlur',
        defaultValues:{
            playerOne:{
                ...leftSideOffer,
                gamePropertyIds: leftSideOffer?.tradeProperties.map( property => property.gamePropertyId)
            },
            playerTwo:{
                ...rightSideOffer,
                gamePropertyIds: rightSideOffer?.tradeProperties.map( property => property.gamePropertyId)
            }
        }
    })

    if(!rightSidePlayer)return

    return (
        <FormProvider {...form}>
            <div className='flex flex-col gap-[20px]'>
                <p className='font-bold'>View Trade</p>

                <div className='flex justify-between'>
                    <div className='flex items-center gap-[20px]'>
                        <img className='w-[30px] h-[30px]' src={leftSidePlayer?.iconUrl}/>
                        <p>{player.playerName}</p>
                    </div>
                    <ArrowLeftRight size={32} />
                    <div className='flex items-center gap-[20px]'>
                        <img className='w-[30px] h-[30px]' src={rightSidePlayer?.iconUrl}/>
                        <p>{rightSidePlayer.playerName}</p>
                    </div>
                    
                </div>
                
                <div className='grid grid-cols-2'>
                    {/* Initiating Trade Player (The Current Client Player) */}
                    <div className='border-r border-black pr-[10px] '>
                        <PlayerOfferView formControlPrefix="playerOne" player={player}/>
                    </div>
                    {/* Other Trade Player */}
                    <div className='border-l border-black pl-[10px] '>
                        <PlayerOfferView formControlPrefix="playerTwo" player={rightSidePlayer}/>
                    </div>
                </div>
                <AdvancedActionButtons
                    buttonConfigs={advancedbuttonConfig}
                />
            </div>
        </FormProvider>
    )
}