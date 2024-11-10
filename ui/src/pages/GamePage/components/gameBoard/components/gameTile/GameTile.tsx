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

    switch (space.boardSpaceCategoryId) {
        case BoardSpaceCategory.Go:
            gameTileComponent = <StartTile/>
            break;
        case BoardSpaceCategory.Jail:
            gameTileComponent = <Jail/>
            break
        case BoardSpaceCategory.FreeParking:
            gameTileComponent = <Vacation/>
            break;
        case BoardSpaceCategory.GoToJail:
            gameTileComponent = <GoToJail/>
            break
        case BoardSpaceCategory.Chance:
            gameTileComponent = <ChanceTile/>
            break
        case BoardSpaceCategory.CommunityChest:
            gameTileComponent = <CommunityChestTile/>
            break
        case BoardSpaceCategory.Railroard:
            gameTileComponent = <Railroad sideClass={sideClass} space={space}/>
            break
        case BoardSpaceCategory.Utility:
            gameTileComponent = <UtilityTile sideClass={sideClass} space={space}/>
            break
        case BoardSpaceCategory.PayTaxes:
            gameTileComponent = <TaxTile/>
            break
        default:
            gameTileComponent = <PropertyTile sideClass={sideClass} position={position}/>
            break;       
    }

    return (
        <div className={`flex hover:scale-[1.05] hover:z-[1] w-full h-full duration-[0.3s] relative`}>
            <div className='absolute w-full h-full'>
                <ul className='absolute top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%] flex space-x-[-5px] z-[1]'>
                    {gameState.players.map((player) => {
                        if(player.boardSpaceId !== position)return null
                        return (
                            <img key={player.id} className='w-[30px] h-[30px] bg-white border border-black rounded-[50%] relative' src={player.iconUrl}/>
                        )
                    })}
                </ul>
                {gameTileComponent}
            </div>
        </div>
    )
}