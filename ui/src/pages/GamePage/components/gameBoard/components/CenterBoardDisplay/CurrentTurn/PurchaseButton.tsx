import { useWebSocket } from "../../../../../../../hooks/useWebSocket"
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider";
import { Player } from "../../../../../../../types/controllers/Player"
import { Property } from "../../../../../../../types/controllers/Property"

export const PurchaseButton:React.FC<{property:Property,player:Player}> = ({property,player}) => {
    
    const {invoke} = useWebSocket();
    const {gameId} = useGameState()

    return (
        <button
            disabled={player.money < property.purchasePrice}
            onClick={() => {
                invoke.gameProperty.update(property.id,{playerId:player.id})
                invoke.player.update(player.id,{money:player.money - property.purchasePrice})
                invoke.gameLog.create(gameId,`${player.playerName} purchased Property`)
            }}
            className='bg-white p-[5px]'
        >
            Purchase (${property.purchasePrice})
        </button>
    )
}