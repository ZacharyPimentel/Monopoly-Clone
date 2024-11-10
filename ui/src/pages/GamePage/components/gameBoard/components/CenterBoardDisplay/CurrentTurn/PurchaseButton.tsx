import { useWebSocket } from "../../../../../../../hooks/useWebSocket"
import { Player } from "../../../../../../../types/controllers/Player"
import { Property } from "../../../../../../../types/controllers/Property"

export const PurchaseButton:React.FC<{property:Property,player:Player}> = ({property,player}) => {
    
    const websocket = useWebSocket();

    return (
        <button
            disabled={player.money < property.purchasePrice}
            onClick={() => {
                websocket.property.update(property.id,{playerId:player.id})
                websocket.player.update(player.id,{money:player.money - property.purchasePrice})
            }}
            className='bg-white p-[5px]'
        >
            Purchase (${property.purchasePrice})
        </button>
    )
}