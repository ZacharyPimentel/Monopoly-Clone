import { useWebSocket } from "../../../../../../../hooks/useWebSocket"
import { Player } from "../../../../../../../types/controllers/Player"
import { Property } from "../../../../../../../types/controllers/Property"

export const PurchaseButton:React.FC<{property:Property,player:Player}> = ({property,player}) => {
    
    const {invoke} = useWebSocket();

    return (
        <button
            disabled={player.money < property.purchasePrice}
            onClick={() => {
                invoke.property.update(property.id,{playerId:player.id})
                invoke.player.update(player.id,{money:player.money - property.purchasePrice})
            }}
            className='bg-white p-[5px]'
        >
            Purchase (${property.purchasePrice})
        </button>
    )
}