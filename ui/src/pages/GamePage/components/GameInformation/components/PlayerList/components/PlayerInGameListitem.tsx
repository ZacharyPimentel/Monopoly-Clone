import { usePlayer } from "@hooks";
import { Player } from "@generated"
import { CreateTradeModal } from "@globalComponents";
import { useGlobalState, useGameState } from "@stateProviders";

export const PlayerInGameListitem:React.FC<{player:Player}> = ({player}) => {
    
    const gameState = useGameState(['game','trades']);
    const currentPlayer = usePlayer();
    const {dispatch:globalDispatch} = useGlobalState([]);

    const currentPlayerTrades = gameState.trades.filter( (trade) => {
        return trade.playerTrades.some( (pt) => pt.playerId === currentPlayer.player?.id)
    })

    const currentPlayerTradesWithPlayer = currentPlayerTrades.filter( trade => {
        return trade.playerTrades.some(pt => pt.playerId === player.id);
    })

    return (
        <li style={{opacity:player.active ? '1' : '0.5'}} key={player.id} className='flex flex-col gap-[20px]'>
            <div className={`flex items-center gap-[20px] border-l-2 pl-[5px] ${player.id === gameState.game?.currentPlayerTurn ? 'border-white' : 'border-transparent'}`}>
                <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                <p className='mr-auto'>{player.playerName} {player.id === currentPlayer.player?.id && '(You)'} {player.bankrupt && '(BANKRUPT)'}</p>
                {currentPlayer.player?.id && currentPlayer.player?.id !== player.id && !player.bankrupt && !currentPlayer.player.bankrupt && currentPlayerTradesWithPlayer.length === 0 && (
                    <button onClick={() => globalDispatch({
                        modalOpen:true,
                        modalContent:<CreateTradeModal tradeWithPlayer={player}/>}
                    )}>
                        <svg className='fill-black' height="24px" viewBox="0 -960 960 960" width="24px" fill="#5f6368"><path d="M280-160 80-360l200-200 56 57-103 103h287v80H233l103 103-56 57Zm400-240-56-57 103-103H440v-80h287L624-743l56-57 200 200-200 200Z"/></svg>
                    </button>
                )}
                <p>${player.money}</p>
            </div>
        </li>
    )
}