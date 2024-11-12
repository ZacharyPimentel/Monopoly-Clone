import { usePlayer } from "./usePlayer"
import { BoardSpaceCategory } from "../types/enums/BoardSpaceCategory";
import { useWebSocket } from "./useWebSocket";
import { useGameState } from "../stateProviders/GameStateProvider";

export const useLandedOnSpace = () => {

    const {player,currentBoardSpace} = usePlayer();
    const gameState = useGameState();
    const websocket = useWebSocket();
    if(!player || player.turnComplete)return

    //=====================
    // Landed On Go
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Go){
        websocket.player.update(player.id,{
            money:player.money + 200,
            turnComplete:true
        })
    }
    //=====================
    // Landed On Property
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Property){
        //no automatic action if player owns the property, or if property is unowned
        if(currentBoardSpace.property?.playerId === player.id)return
        if(!currentBoardSpace.property?.playerId) return
        const paymentAmount = currentBoardSpace.property.propertyRents[currentBoardSpace.property.upgradeCount].rent
        //take money from player who lands on property
        websocket.player.update(player.id,{
            money: player.money - paymentAmount,
            turnComplete:true
        })
        //give money to property owner
        const propertyOwner = gameState.players.find( (player) => player.id === currentBoardSpace.property?.playerId)!
        websocket.player.update(propertyOwner.id,{money:propertyOwner.money + paymentAmount})
    }
    //=====================
    // Landed On Railroad
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Railroard){
        //no automatic action if player owns the property, or if property is unowned
        if(currentBoardSpace.property?.playerId === player.id)return
        if(!currentBoardSpace.property?.playerId) return
        const ownerRailroads = gameState.boardSpaces.filter( (space) => {
            space.boardSpaceCategoryId === BoardSpaceCategory.Railroard &&
            space.property?.playerId === currentBoardSpace.property?.playerId
        });
        //calculate based on number of railroads the cost (25,50,100,or 200)
        let paymentAmount = 25 * (2^(ownerRailroads.length - 1));
        
        //take money from player who lands on property
        websocket.player.update(player.id,{
            money: player.money - paymentAmount,
            turnComplete:true
        })
        //give money to property owner
        const propertyOwner = gameState.players.find( (player) => player.id === currentBoardSpace.property?.playerId)!
        websocket.player.update(propertyOwner.id,{money:propertyOwner.money + paymentAmount})
    }
    //=====================
    // Landed On Go To Jail
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.GoToJail){
        websocket.player.update(player.id,{
            boardSpaceId:11,
            inJail:true
        })
    }
}