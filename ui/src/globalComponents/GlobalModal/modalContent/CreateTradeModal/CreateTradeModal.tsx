import { ActionButtons } from "@globalComponents"
import { usePlayer, useTradeForm, useWebSocket } from "@hooks"
import {FormProvider} from 'react-hook-form'
import { Player } from "@generated"
import { useMemo } from "react"
import { useGameState } from "@stateProviders"
import { ArrowLeftRight } from "lucide-react"
import { PlayerOfferEdit } from "./components/PlayerOfferEdit"

export const CreateTradeModal:React.FC<{tradeWithPlayer:Player}> = ({tradeWithPlayer}) => {

    const gameState = useGameState(['gameId', 'boardSpaces']);
    const {player} = usePlayer()
    const {invoke} = useWebSocket();

    const form = useTradeForm({tradePartnerId:tradeWithPlayer.id})
 
    const defaultValues = useMemo(() => {
        return form.getValues()
    },[])

    const tradeHasSomethingFilledOut = useMemo( () => {
        let defaultsHaveChanged = JSON.stringify(form.watch()) !== JSON.stringify(defaultValues)
        return defaultsHaveChanged
    },[form.watch()])

    return (
        <FormProvider {...form}>
            <div className='flex flex-col gap-[20px]'>
                <p className='font-bold'>Create a Trade</p>
                
                <div className='flex justify-between'>
                    <div className='flex items-center gap-[20px]'>
                        <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                        <p>{player.playerName}</p>
                    </div>
                    <ArrowLeftRight size={32} />
                    <div className='flex items-center gap-[20px]'>
                        <img className='w-[30px] h-[30px]' src={tradeWithPlayer.iconUrl}/>
                        <p>{tradeWithPlayer.playerName}</p>
                    </div>
                    
                </div>
                
                <div className='grid grid-cols-2'>
                    {/* Initiating Trade Player (The Current Client Player) */}
                    <div className='border-r border-black pr-[10px] flex flex-col'>
                        <PlayerOfferEdit formControlPrefix="playerOne" player={player}/>
                    </div>
                    {/* Other Trade Player */}
                    <div className='border-l border-black pl-[10px] '>
                        <PlayerOfferEdit formControlPrefix="playerTwo" player={tradeWithPlayer}/>
                    </div>
                </div>
                <ActionButtons
                    confirmButtonStyle="success"
                    confirmCallback={async() => {
                        invoke.trade.create({
                            gameId: gameState.gameId,
                            initiator: form.getValues('playerOne').playerId,
                            playerOne: form.getValues('playerOne'),
                            playerTwo: form.getValues('playerTwo')
                        })
                    }}
                    confirmButtonText="Create Trade"
                    confirmDisabled={!tradeHasSomethingFilledOut}
                />
            </div>
        </FormProvider>
    )
}