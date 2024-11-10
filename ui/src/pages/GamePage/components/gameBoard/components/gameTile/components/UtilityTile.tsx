import { useGameState } from "../../../../../../../stateProviders/GameStateProvider";
import { BoardSpace } from "../../../../../../../types/controllers/BoardSpace";

export const UtilityTile:React.FC<{space:BoardSpace,sideClass:string}> = ({space,sideClass}) => {

    const gameState = useGameState();

    if(!space.property)return null

    const property = space.property;

    return (
        <button className={`${sideClass} w-full h-full bg-[yellow] flex items-center justify-between shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden`}>
            {property.playerId
                ? <img className='w-[30px] h-[30px] opacity-[0.7]' src={gameState.players.find( (player) => player.id === property.playerId)?.iconUrl}/>
                : <p className='text-center bg-[#eaeaea]'>${property.purchasePrice}</p>

            }
            <p className='absolute top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%]'>Utility</p>
        </button>
    )
}