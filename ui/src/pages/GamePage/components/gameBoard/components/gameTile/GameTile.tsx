import { chanceTilePositions, communityChestTilePositions, railroadTilePositions, taxTilePositions, utilityTilePositions } from "../../../../../../helpers/tilePositions";
import { ChanceTile } from "./components/ChanceTile";
import { CommunityChestTile } from "./components/CommunityChestTile";
import { GoToJail } from "./components/GoToJail"
import { Jail } from "./components/Jail"
import { PropertyTile } from "./components/PropertyTile";
import { Railroad } from "./components/Railroad";
import { StartTile } from "./components/StartTile"
import { TaxTile } from "./components/TaxTile";
import { UtilityTile } from "./components/UtilityTile";
import { Vacation } from "./components/Vacation"

export const GameTile:React.FC<{position:number}> = ({position}) => {

    let gameTileComponent = null;

    switch (position) {
        case 1:
            gameTileComponent = <StartTile/>
            break;
        case 11:
            gameTileComponent = <Jail/>
            break;
        case 21:
            gameTileComponent = <Vacation/>
            break;
        case 31:
            gameTileComponent = <GoToJail/>
            break
        case chanceTilePositions.includes(position) && position:
            gameTileComponent = <ChanceTile/>
            break
        case communityChestTilePositions.includes(position) && position:
            gameTileComponent = <CommunityChestTile/>
            break
        case railroadTilePositions.includes(position) && position:
            gameTileComponent = <Railroad position={position}/>
            break
        case utilityTilePositions.includes(position) && position:
            gameTileComponent = <UtilityTile/>
            break
        case taxTilePositions.includes(position) && position:
            gameTileComponent = <TaxTile/>
            break
        default:
            gameTileComponent = <PropertyTile position={position}/>
            break;       
    }

    return (
        <div className={`flex hover:scale-[1.05] hover:z-[1] w-full h-full duration-[0.3s] relative`}>
            <div className='absolute w-full h-full'>
                {gameTileComponent}
            </div>
        </div>
    )
}