import { usePlayer, useTradeForm, useWebSocket } from "@hooks"
import { FormProvider } from 'react-hook-form'
import React, { useMemo } from "react"
import { PlayerOfferView } from "./components/PlayerOfferView"
import { AdvancedActionButtons, AdvancedButtonConfig } from "@globalComponents"
import { useGameState, useGlobalState } from "@stateProviders"
import { ArrowLeftRight } from "lucide-react"
import { Trade } from "@generated"
import { EditTradeModal } from "./EditTradeModal"

export const ViewTradeModal:React.FC<{trade:Trade}> = ({trade}) => {

    const gameState = useGameState(['players'])
    const {dispatch} = useGlobalState([]);
    const {player} = usePlayer()
    const {invoke} = useWebSocket();

    const form = useTradeForm({trade});

    const leftSidePlayer = gameState.players.find( p => p.id === form.getValues("playerOne.playerId"))!

    const rightSidePlayer = gameState.players.find( x => x.id === form.getValues("playerTwo.playerId"))!

    const advancedbuttonConfig:AdvancedButtonConfig[] =  useMemo( () => {
        
        const config:AdvancedButtonConfig[] = [];

        //if not a member of the trade, should not see any action buttons
        if(leftSidePlayer?.id !== player.id && rightSidePlayer?.id !== player.id){
            return config
        }

        // allow modify trade
        if(player.id !== trade.lastUpdatedBy){
            config.push({
                buttonText:'Modify',
                buttonStyle:'success',
                closeOnAction:false,
                buttonCallback: async() => {
                    dispatch({modalContent: <EditTradeModal trade={trade}/>, modalOpen:true})
                }
            })
            config.push({
                buttonText:'Accept',
                buttonStyle:'success',
                closeOnAction:true,
                buttonCallback: async() => {
                    console.log(trade)
                    invoke.trade.accept({tradeId:trade.id})
                }
            })
        }
        config.push({
            buttonText:'Decline',
            buttonStyle:'warning',
            closeOnAction:true,
            buttonCallback: async() => {
                invoke.trade.decline({ tradeId: trade.id });
            }
        })
        return config;
    },[])

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
                        <PlayerOfferView formControlPrefix="playerOne" player={leftSidePlayer}/>
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