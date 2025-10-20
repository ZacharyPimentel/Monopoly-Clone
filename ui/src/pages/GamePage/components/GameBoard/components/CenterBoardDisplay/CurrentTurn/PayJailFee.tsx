import { Player } from "@generated";
import { useWebSocket } from "@hooks"

export const PayJailFee:React.FC<{player:Player}> = ({player}) => {
    
    const {invoke} = useWebSocket();

    const canAffordToPay = player.money >= 50
    const hasGetOutOfJailFree = player.getOutOfJailFreeCards > 0

    if(!canAffordToPay && !hasGetOutOfJailFree) return null

    return (
        <>
            {canAffordToPay && (
                <button
                    onClick={() => {
                        invoke.player.payOutOfJail();
                    }}
                    className='bg-white p-[5px]'
                >
                    Pay $50
                </button>
                )}
            {hasGetOutOfJailFree && (
                <button
                    onClick={() => {
                        invoke.player.getOutOfJailFree();
                    }}
                    className='bg-white p-[5px] w-full'
                >
                    Use get out of jail free
                </button>
            )}
        </>
    )
}