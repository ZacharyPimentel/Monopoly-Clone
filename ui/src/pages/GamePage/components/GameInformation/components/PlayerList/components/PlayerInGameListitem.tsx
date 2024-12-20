import { usePlayer } from "../../../../../../../hooks/usePlayer";
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider"
import { useGlobalDispatch } from "../../../../../../../stateProviders/GlobalStateProvider";
import { Player } from "../../../../../../../types/controllers/Player"
import { CreateTradeModal } from "../../Trades/modal/CreateTradeModal";

export const PlayerInGameListitem:React.FC<{player:Player}> = ({player}) => {
    
    const gameState = useGameState();
    const currentPlayer = usePlayer();
    const globalDispatch = useGlobalDispatch();

    return (
        <li style={{opacity:player.active ? '1' : '0.5'}} key={player.id} className='flex flex-col gap-[20px]'>
            <div className={`flex items-center gap-[20px] border-l-2 pl-[5px] ${player.id === gameState.game?.currentPlayerTurn ? 'border-white' : 'border-transparent'}`}>
                <img className='w-[30px] h-[30px]' src={player.iconUrl}/>
                <p className='mr-auto'>{player.playerName} {player.id === currentPlayer.player?.id && '(You)'}</p>
                {currentPlayer.player?.id && currentPlayer.player?.id !== player.id && (
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