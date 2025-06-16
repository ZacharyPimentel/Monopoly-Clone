import { useGameState } from "@stateProviders/GameStateProvider";
import { BoardSpaceCategories } from "@generated/index";
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
    if(!space)return null

    switch (space.boardSpaceCategoryId) {
        case BoardSpaceCategories.Go:
            gameTileComponent = <StartTile space={space}/>
            break;
        case BoardSpaceCategories.Jail:
            gameTileComponent = <Jail space={space}/>
            break
        case BoardSpaceCategories.FreeParking:
            gameTileComponent = <Vacation space={space}/>
            break;
        case BoardSpaceCategories.GoToJail:
            gameTileComponent = <GoToJail space={space}/>
            break
        case BoardSpaceCategories.Chance:
            gameTileComponent = <ChanceTile space={space}/>
            break
        case BoardSpaceCategories.CommunityChest:
            gameTileComponent = <CommunityChestTile space={space}/>
            break
        case BoardSpaceCategories.Railroard:
            gameTileComponent = <Railroad sideClass={sideClass} space={space}/>
            break
        case BoardSpaceCategories.Utility:
            gameTileComponent = <UtilityTile sideClass={sideClass} space={space}/>
            break
        case BoardSpaceCategories.PayTaxes:
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
                    {space.boardSpaceCategoryId !== BoardSpaceCategories.Jail && 
                        gameState.players
                            .filter((player) => player.inCurrentGame)
                            //Filter out current player if movement is in progress
                            .filter((player) => {
                                if(gameState?.game?.movementInProgress){
                                    if(player.id == gameState?.game?.currentPlayerTurn){
                                        return false
                                    }
                                    return true
                                }else{
                                    return true
                                }
                            })
                            .map((player) => {
                                if(player.boardSpaceId !== position)return null
                                const isThisPlayersTurn = gameState.game?.currentPlayerTurn === player.id;
                                return (
                                    <div key={player.id} className='relative' >
                                        <span className={`${isThisPlayersTurn ? 'bg-pink-500 animate-ping' : ''} absolute w-full h-full rounded-[50%]`}></span>
                                        <img 
                                            className='w-[30px] h-[30px] bg-white border border-black rounded-[50%] relative pointer-events-none' 
                                            src={player.iconUrl}
                                        />
                                    </div>
                                )
                            }
                    )}
                </ul>
                {gameTileComponent}
            </div>
        </div>
    )
}