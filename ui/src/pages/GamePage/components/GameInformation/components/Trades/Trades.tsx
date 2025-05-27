import { useGlobalDispatch } from "../../../../../../stateProviders/GlobalStateProvider"
import { CreateTradeModal } from "../../../../../../globalComponents/GlobalModal/modalContent/CreateTradeModal";
import { useGameState } from "../../../../../../stateProviders/GameStateProvider";
import { Fragment } from "react";
import { EditTradeModal } from "../../../../../../globalComponents/GlobalModal/modalContent/EditTradeModal";

export const Trades = () => {

    const gameState = useGameState()
    const globalDispatch = useGlobalDispatch()

    return (
        <div className='flex flex-col gap-[10px] p-[30px] bg-totorogreen w-full'>
            <div className='flex justify-between gap-[20px]'>
                <p>Trades</p>
            </div>
            <ul>
                {gameState.trades.map( (trade) => {
                    return (
                        <li key={trade.id} className='flex justify-between gap-[20px]'>
                            <div className='flex gap-[20px] justify-around items-center w-full'>
                                {trade.playerTrades.map( (playerTrade,index) => {
                                    const player = gameState.players.find( x => x.id === playerTrade.playerId);
                                    if(!player)return
                                    return (<Fragment key={index}>
                                        {index === 1 && (
                                            <svg className='fill-black' xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 -960 960 960" width="24px" fill="#5f6368"><path d="M280-160 80-360l200-200 56 57-103 103h287v80H233l103 103-56 57Zm400-240-56-57 103-103H440v-80h287L624-743l56-57 200 200-200 200Z"/></svg>
                                        )}
                                        <div className='flex items-center gap-[10px]'>
                                            <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                                            {player.playerName}
                                        </div>
                                    </Fragment>)
                                })}
                                <button onClick={() => globalDispatch({modalOpen:true,modalContent:<EditTradeModal trade={trade}/>})} className='p-[5px] border ml-auto'>View</button>
                            </div>
                        </li>
                    )
                })}
            </ul>
            
        </div>
        
    )
}