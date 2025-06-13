import { useWebSocket } from "@hooks/useWebSocket"
import { Player,Property } from "@generated/index"

export const PurchaseButton:React.FC<{property:Property,player:Player,spaceName:string}> = ({property,player,spaceName}) => {
    
    const {invoke} = useWebSocket();

    return (
        <button
            disabled={player.money < property.purchasePrice}
            onClick={() => {
                invoke.player.purchaseProperty({gamePropertyId:property.id})
            }}
            className='bg-white p-[5px] disabled:opacity-[0.6]'
        >
            Purchase (${property.purchasePrice})
        </button>
    )
}