import { usePlayer } from "../../../../../../../hooks/usePlayer"
import { useWebSocket } from "../../../../../../../hooks/useWebSocket"
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider";

export const PayJailFee:React.FC = () => {
    
    const {player} = usePlayer();
    const {invoke} = useWebSocket();
    const {gameId} = useGameState();

    return (
        <button
            onClick={() => {
                invoke.player.update(player.id,{
                    money:player.money - 50,
                    inJail:false,
                    rollCount:0
                })
                invoke.gameLog.create(gameId, `${player.playerName} paid $50 to get out of jail.`)
            }}
            className='bg-white p-[5px]'
        >
            Pay $50
        </button>
    )
}