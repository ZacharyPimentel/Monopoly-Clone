import { useGameState } from "../stateProviders/GameStateProvider";
import { Player } from "../types/controllers/Player";
import { BoardSpaceCategory } from "../types/enums/BoardSpaceCategory";

export const useLandedSpaceAction = (currentPlayer:Player | undefined) => {

    const gameState = useGameState();

    if(!currentPlayer || currentPlayer.rollCount === 0) return null

    const landedSpaceCategory = gameState.boardSpaces[currentPlayer.boardSpaceId-1].boardSpaceCategoryId;
    console.log('11',currentPlayer.boardSpaceId)
    console.log('12',landedSpaceCategory)
    console.log(gameState.boardSpaces)
    //if player landed on a property tile
    if(landedSpaceCategory === BoardSpaceCategory.Property){
        const currentProperty = gameState.boardSpaces[currentPlayer.boardSpaceId-1].property;
        console.log('16',currentProperty)
        if(currentProperty?.playerId){
            //pay the player who owns the property
            return null
        }else{
            return currentProperty
        }
    }
    if(landedSpaceCategory === BoardSpaceCategory.Railroard){
        const currentProperty = gameState.boardSpaces[currentPlayer.boardSpaceId-1].property;
        if(currentProperty?.playerId){
            //pay the player who owns the Railroad
            return null
        }else{
            return currentProperty
        }
    }
    if(landedSpaceCategory === BoardSpaceCategory.Utility){
        const currentProperty = gameState.boardSpaces[currentPlayer.boardSpaceId-1].property;
        if(currentProperty?.playerId){
            //pay the player who owns the Utility
            return null
        }else{
            return currentProperty
        }
    }

    return null
}