import { useWebSocket } from "@hooks/useWebSocket"

export const PayJailFee:React.FC = () => {
    
    const {invoke} = useWebSocket();

    return (
        <button
            onClick={() => {
                invoke.player.payOutOfJail();
            }}
            className='bg-white p-[5px]'
        >
            Pay $50
        </button>
    )
}