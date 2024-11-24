import { useGameState } from "../../../../../../stateProviders/GameStateProvider";
import { BoardSpaceCategory } from "../../../../../../types/enums/BoardSpaceCategory";
import { ChanceTile } from "./components/ChanceTile";
import { CommunityChestTile } from "./components/CommunityChestTile";
import { GoToJail } from "./components/GoToJail";
import { Jail } from "./components/Jail";
import { PropertyTile } from "./components/PropertyTile";
import { Railroad } from "./components/Railroad";
import { StartTile } from "./components/StartTile";
import { TaxTile } from "./components/TaxTile";
import { UtilityTile } from "./components/UtilityTile";
import { Vacation } from "./components/Vacation";

export const GameTile:React.FC<{position:number, sideClass?:string}> = ({position,sideClass = ''}) => {

    let gameTileComponent = null;
    const gameState = useGameState();
    const space = gameState.boardSpaces[position - 1];
    console.log(space)
    if(!space)return null

    switch (space.boardSpaceCategoryId) {
        case BoardSpaceCategory.Go:
            gameTileComponent = <StartTile space={space}/>
            break;
        case BoardSpaceCategory.Jail:
            gameTileComponent = <Jail space={space}/>
            break
        case BoardSpaceCategory.FreeParking:
            gameTileComponent = <Vacation space={space}/>
            break;
        case BoardSpaceCategory.GoToJail:
            gameTileComponent = <GoToJail space={space}/>
            break
        case BoardSpaceCategory.Chance:
            gameTileComponent = <ChanceTile space={space}/>
            break
        case BoardSpaceCategory.CommunityChest:
            gameTileComponent = <CommunityChestTile space={space}/>
            break
        case BoardSpaceCategory.Railroard:
            gameTileComponent = <Railroad sideClass={sideClass} space={space}/>
            break
        case BoardSpaceCategory.Utility:
            gameTileComponent = <UtilityTile sideClass={sideClass} space={space}/>
            break
        case BoardSpaceCategory.PayTaxes:
            gameTileComponent = <TaxTile space={space}/>
            break
        default:
            gameTileComponent = <PropertyTile sideClass={sideClass} position={position}/>
            break;       
    }

    return (
        <div className={`flex hover:scale-[1.05] hover:z-[1] w-full h-full duration-[0.3s] relative`}>
            <div className='absolute w-full h-full group'>
                <ul className={`absolute top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%] flex space-x-[-5px] z-[1]`}>
                    {space.boardSpaceCategoryId !== BoardSpaceCategory.Jail && gameState.players.map((player) => {
                        if(player.boardSpaceId !== position)return null
                        return (
                            <img key={player.id} className='w-[30px] h-[30px] bg-white border border-black rounded-[50%] relative pointer-events-none' src={player.iconUrl}/>
                        )
                    })}
                </ul>
                {gameTileComponent}
            </div>
        </div>
    )
}