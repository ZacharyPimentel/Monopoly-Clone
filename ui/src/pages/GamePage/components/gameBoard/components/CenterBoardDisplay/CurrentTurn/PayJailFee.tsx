import { usePlayer } from "../../../../../../../hooks/usePlayer"
import { useWebSocket } from "../../../../../../../hooks/useWebSocket"

export const PayJailFee:React.FC = () => {
    
    const {player} = usePlayer();
    const {invoke} = useWebSocket();

    return (
        <button
            onClick={() => {
                invoke.player.update(player.id,{
                    money:player.money - 50,
                    inJail:false,
                    rollCount:0
                })
            }}
            className='bg-white p-[5px]'
        >
            Pay $50
        </button>
    )
}