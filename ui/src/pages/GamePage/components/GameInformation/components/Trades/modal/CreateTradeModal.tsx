import { useMemo, useState } from "react"
import { ActionButtons } from "../../../../../../../globalComponents/GlobalModal/ActionButtons"
import { usePlayer } from "../../../../../../../hooks/usePlayer"
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider"
import { OptionSelectMenu } from "../../../../../../../globalComponents/FormElements/OptionSelectMenu"
import { useApi } from "../../../../../../../hooks/useApi"
import { Player } from "../../../../../../../types/controllers/Player"

export const CreateTradeModal:React.FC<{tradeWithPlayer:Player}> = ({tradeWithPlayer}) => {

    const gameState = useGameState()
    const {player} = usePlayer()
    const [tradingPlayerId,setTradingPlayerId] = useState('');
    const api = useApi();

    const tradeWithPlayerProperties = useMemo( () => {
        return gameState.boardSpaces.filter( (space) => space.property && space.property?.playerId === tradeWithPlayer.id);
    },[tradeWithPlayer])

    const playerProperties = useMemo( () => {
        return gameState.boardSpaces.filter( (space) => space.property && space.property?.playerId === player.id);
    },[tradeWithPlayer])

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Create a Trade</p>
            
            <div className='grid grid-cols-2'>
                <div className='border-r border-black pr-[10px] flex flex-col gap-[20px]' >
                    <div className='flex items-center gap-[20px]'>
                        <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                        <p>{player.playerName}</p>
                    </div>
                    <div className='flex flex-col'>
                        <p>Properties:</p>
                        {playerProperties.length > 0 && playerProperties.map( (space) => {
                            return (
                                <div>{space.boardSpaceName}</div>
                            )
                        })}
                        {playerProperties.length === 0 && <p className='opacity-[0.5]'><i>No properties to trade</i></p>}
                    </div>
                    <label className='flex flex-wrap gap-x-[10px] items-center'>
                        <p className=''>Money</p>
                        <input className='flex-1 min-w-[50%]' type='text'/>
                    </label>
                </div>
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
                    <label className='flex flex-wrap gap-x-[10px] items-center'>
                        <p className=''>Money</p>
                        <input className='flex-1 min-w-[50%]' type='text'/>
                    </label>
                    <p>Get Out Of Jail Free Cards</p>
                </div>
            </div>
            <ActionButtons
                confirmButtonStyle="success"
                confirmCallback={async() => {}}
                confirmButtonText="Create Trade"
                confirmDisabled={!tradingPlayerId}
            />
        </div>
    )
}