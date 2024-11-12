import { usePlayer } from "../../../../../../../hooks/usePlayer"
import { useWebSocket } from "../../../../../../../hooks/useWebSocket"

export const PayJailFee:React.FC = () => {
    
    const {player} = usePlayer();
    const websocket = useWebSocket();

    return (
        <button
            onClick={() => {
                websocket.player.update(player.id,{
                    money:player.money - 50,
                    inJail:false,
                    turnComplete:true
                })
            }}
            className='bg-white p-[5px]'
        >
            Pay $50
        </button>
    )
}