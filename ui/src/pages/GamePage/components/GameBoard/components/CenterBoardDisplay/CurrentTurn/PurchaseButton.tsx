import { useWebSocket } from "@hooks"
import { Player,Property } from "@generated"

export const PurchaseButton:React.FC<{property:Property,player:Player,spaceName:string}> = ({property,player}) => {
    
    const {invoke} = useWebSocket();

    return (
        <button
            disabled={player.money < property.purchasePrice}
            onClick={() => {
                if(!property.gamePropertyId)return
                invoke.player.purchaseProperty({gamePropertyId:property.gamePropertyId})
            }}
            className='game-button'
        >
            Purchase (${property.purchasePrice})
        </button>
    )
}