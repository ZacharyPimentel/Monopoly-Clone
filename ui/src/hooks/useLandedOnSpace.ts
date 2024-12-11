import { usePlayer } from "./usePlayer"
import { BoardSpaceCategory } from "../types/enums/BoardSpaceCategory";
import { useWebSocket } from "./useWebSocket";
import { useGameState } from "../stateProviders/GameStateProvider";
import { useEffect, useState } from "react";

export const useLandedOnSpace = () => {

    const {player,currentBoardSpace} = usePlayer();
    const gameState = useGameState();
    const {invoke} = useWebSocket();
    const [lastBoardSpace,setLastBoardSpace] = useState(currentBoardSpace)

    if(currentBoardSpace === lastBoardSpace) return
    if(!player || player.turnComplete || player.rollCount === 0 || gameState.rolling)return

    //=====================
    // Landed On Go
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Go){
        invoke.player.update(player.id,{
            money:player.money + 100,
            turnComplete:true
        })
        invoke.gameLog.create(gameState.gameId,`${player.playerName} made an extra $100 for landing on GO.`)
    }
    //=====================
    // Landed On Property
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Property){
        //no automatic action if player owns the property, or if property is unowned
        if(currentBoardSpace.property?.playerId === player.id)return
        if(!currentBoardSpace.property?.playerId)return
        
        const paymentAmount = currentBoardSpace.property.propertyRents[currentBoardSpace.property.upgradeCount].rent
        //take money from player who lands on property
        invoke.player.update(player.id,{
            money: player.money - paymentAmount,
            turnComplete:true
        })
        //give money to property owner
        const propertyOwner = gameState.players.find( (player) => player.id === currentBoardSpace.property?.playerId)!
        invoke.player.update(propertyOwner.id,{money:propertyOwner.money + paymentAmount})
        invoke.gameLog.create(gameState.gameId,`${player.playerName} paid ${propertyOwner.playerName} $${paymentAmount}.`)
    }
    //=====================
    // Landed On Railroad
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Railroard){
        //no automatic action if player owns the property, or if property is unowned
        if(currentBoardSpace.property?.playerId === player.id)return
        if(!currentBoardSpace.property?.playerId) return

        const ownerRailroads = gameState.boardSpaces.filter( (space) => 
            space.boardSpaceCategoryId === BoardSpaceCategory.Railroard &&
            space.property?.playerId === currentBoardSpace.property?.playerId
        );
        //calculate based on number of railroads the cost (25,50,100,or 200)
        let paymentAmount = ownerRailroads.length > 0 ? 25 * Math.pow(2, ownerRailroads.length - 1) : 0;
        
        //take money from player who lands on property
        invoke.player.update(player.id,{
            money: player.money - paymentAmount,
            turnComplete:true
        })
        //give money to property owner
        const propertyOwner = gameState.players.find( (player) => player.id === currentBoardSpace.property?.playerId)!
        invoke.player.update(propertyOwner.id,{money:propertyOwner.money + paymentAmount})
        invoke.gameLog.create(gameState.gameId,`${player.playerName} paid ${propertyOwner.playerName} $${paymentAmount}.`)
    }
    //=====================
    // Landed On Go To Jail
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.GoToJail){
        invoke.player.update(player.id,{
            boardSpaceId: 11, //id for jail
            inJail:true,
            turnComplete:true,
            rollCount:1
        })
        invoke.gameLog.create(gameState.gameId,`${player.playerName} went to jail.`)
    }
    //=====================
    // Landed On Utility
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Utility){
        //no automatic action if player owns the property, or if property is unowned
        if(currentBoardSpace.property?.playerId === player.id)return
        if(!currentBoardSpace.property?.playerId) return
        console.log('update player to roll for utils')
        invoke.player.update(player.id,{rollingForUtilities:true})
    }
    //=====================
    // Landed On Tax
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.PayTaxes){
        let paymentAmount = 0;
        //income tax, 10%
        if(currentBoardSpace.id === 5){
            paymentAmount = Math.round(player.money * 0.1)
        }
        //luxury tax, $75
        else{
            paymentAmount = 75
        }
        
        invoke.player.update(player.id,{
            money: player.money - paymentAmount,
            turnComplete:true
        })
        invoke.gameLog.create(gameState.gameId,`${player.playerName} paid $${paymentAmount} in taxes.`)
    }
    //=====================
    // Landed On Free Parking
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.FreeParking){
        invoke.player.update(player.id,{turnComplete:true})
    }
    //=====================
    // Landed On Chance
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Chance){
        invoke.player.update(player.id,{turnComplete:true})
    }
    //=====================
    // Landed On Free Parking
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.CommunityChest){
        invoke.player.update(player.id,{turnComplete:true})
    }
    //=====================
    // Landed On Jail
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Jail){
        invoke.player.update(player.id,{turnComplete:true})
    }
    setLastBoardSpace(currentBoardSpace)
}