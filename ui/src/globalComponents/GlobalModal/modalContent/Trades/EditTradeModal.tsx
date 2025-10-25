import { ActionButtons, AdvancedActionButtons, AdvancedButtonConfig, ViewTradeModal } from "@globalComponents"
import { usePlayer, useTradeForm, useWebSocket } from "@hooks"
import {FormProvider} from 'react-hook-form'
import { Trade } from "@generated"
import { useMemo } from "react"
import { useGameState, useGlobalState } from "@stateProviders"
import { ArrowLeftRight } from "lucide-react"
import { PlayerOfferEdit } from "./components/PlayerOfferEdit"

export const EditTradeModal:React.FC<{trade:Trade}> = ({trade}) => {

    const gameState = useGameState(['gameId', 'boardSpaces','players']);
    const {dispatch} = useGlobalState([]);
    const {invoke} = useWebSocket();

    const form = useTradeForm({trade})
 
    const defaultValues = useMemo(() => {
        return form.getValues()
    },[])

    const tradeHasSomethingFilledOut = useMemo( () => {
        let defaultsHaveChanged = JSON.stringify(form.watch()) !== JSON.stringify(defaultValues)
        return defaultsHaveChanged
    },[form.watch()])

    const leftSidePlayer = gameState.players.find( p => p.id === form.getValues("playerOne.playerId"))!

    const rightSidePlayer = gameState.players.find( x => x.id === form.getValues("playerTwo.playerId"))!

    const advancedbuttonConfig:AdvancedButtonConfig[] =  useMemo( () => {
            const config:AdvancedButtonConfig[] = [];
            config.push({
                buttonText:'Go Back',
                buttonStyle:'warning',
                closeOnAction:false,
                buttonCallback: async() => {
                    dispatch({modalContent: <ViewTradeModal trade={trade}/>})
                }
            })
            config.push({
                buttonText:'Update',
                buttonStyle:'success',
                closeOnAction:true,
                disabled: !tradeHasSomethingFilledOut,
                buttonCallback: async() => {
                    invoke.trade.update({
                        tradeId: trade.id,
                        playerOne:form.getValues().playerOne,
                        playerTwo:form.getValues().playerTwo
                    })
                }
            })
            return config;
        },[tradeHasSomethingFilledOut])

    return (
        <FormProvider {...form}>
            <div className='flex flex-col gap-[20px]'>
                <p className='font-bold'>Update Trade</p>
                
                <div className='flex justify-between'>
                    <div className='flex items-center gap-[20px]'>
                        <img className='w-[30px] h-[30px]' src={leftSidePlayer.iconUrl}/>
                        <p>{leftSidePlayer.playerName}</p>
                    </div>
                    <ArrowLeftRight size={32} />
                    <div className='flex items-center gap-[20px]'>
                        <img className='w-[30px] h-[30px]' src={rightSidePlayer.iconUrl}/>
                        <p>{rightSidePlayer.playerName}</p>
                    </div>
                    
                </div>
                
                <div className='grid grid-cols-2'>
                    {/* Initiating Trade Player (The Current Client Player) */}
                    <div className='border-r border-black pr-[10px]'>
                        <PlayerOfferEdit formControlPrefix="playerOne" player={leftSidePlayer}/>
                    </div>
                    {/* Other Trade Player */}
                    <div className='border-l border-black pl-[10px] '>
                        <PlayerOfferEdit formControlPrefix="playerTwo" player={rightSidePlayer}/>
                    </div>
                </div>
                <AdvancedActionButtons buttonConfigs={advancedbuttonConfig}/>
                {/* <ActionButtons
                    confirmButtonStyle="success"
                    confirmCallback={async() => {
                        invoke.trade.update({
                            tradeId: trade.id,
                            playerOne:form.getValues().playerOne,
                            playerTwo:form.getValues().playerTwo
                        })
                    }}
                    confirmButtonText="Update Trade"
                    confirmDisabled={!tradeHasSomethingFilledOut}
                /> */}
            </div>
        </FormProvider>
    )
}