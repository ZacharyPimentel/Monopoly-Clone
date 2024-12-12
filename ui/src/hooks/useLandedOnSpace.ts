import { usePlayer } from "./usePlayer"
import { BoardSpaceCategory } from "../types/enums/BoardSpaceCategory";
import { useWebSocket } from "./useWebSocket";
import { useGameDispatch, useGameState } from "../stateProviders/GameStateProvider";
import { useState } from "react";
import { useApi } from "./useApi";
import { CardTypeId } from "../types/enums/CardTypeId";
import { GameCard } from "../types/controllers/GameCard";
import { CardActionId } from "../types/enums/CardActionId";

export const useLandedOnSpace = () => {

    const {player,currentBoardSpace} = usePlayer();
    const gameState = useGameState();
    const api = useApi();
    const {invoke} = useWebSocket();
    const [lastBoardSpace,setLastBoardSpace] = useState(currentBoardSpace)
    const gameDispatch = useGameDispatch();

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
    //=====================================
    // Landed On Chance Or Community Chest
    //=====================================
    if(
        currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Chance || 
        currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.CommunityChest
    ){
        //pull a card and handle it
        (async() => {
            const cardType = currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Chance ?  CardTypeId.Chance :  CardTypeId.CommunityChest
            const card:GameCard = await api.gameCard.getOne(gameState.gameId, cardType);

            //Pay bank
            if(card.card.cardActionId === CardActionId.PayBank){
                const payAmount = card.card.amount || 0;
                invoke.player.update(player.id,{money: player.money - payAmount,turnComplete:true})
                invoke.gameLog.create(gameState.gameId,`${player.playerName} lost $${payAmount}.`)
            }
            //Get from bank
            if(card.card.cardActionId === CardActionId.ReceiveFrombank){
                const amount = card.card.amount || 0;
                invoke.player.update(player.id,{money: player.money + amount,turnComplete:true})
                invoke.gameLog.create(gameState.gameId,`${player.playerName} received $${amount}.`)
            }
            //advance to space
            if(card.card.cardActionId === CardActionId.AdvanceToSpace){
                const space = gameState.boardSpaces.find((space) => space.id === card.card.advanceToSpaceId)
                if(!space){
                    console.error('No space found in useLandedOnSpace chance/community chest handler advance to space')
                    return
                }
                invoke.player.update(player.id,{boardSpaceId:space.id})
                invoke.gameLog.create(gameState.gameId,`${player.playerName} has advanced to ${space.boardSpaceName}.`)
            }
            //back 3 spaces
            if(card.card.cardActionId === CardActionId.BackThreeSpaces){
                let newSpaceId = player.boardSpaceId - 3;
                if (newSpaceId <= 0){
                    newSpaceId = 40 - Math.abs(newSpaceId)
                }
                const space = gameState.boardSpaces.find((space) => space.id === newSpaceId)
                if(!space){
                    console.error('No space found in useLandedOnSpace chance/community chest handler back three spaces')
                    return
                }
                invoke.player.update(player.id,{boardSpaceId:space.id})
                invoke.gameLog.create(gameState.gameId,`${player.playerName} went back three spaces to ${space.boardSpaceName}.`)
            }
            //straight to jail
            if(card.card.cardActionId === CardActionId.GoToJail){
                invoke.player.update(player.id,{
                    boardSpaceId:11,
                    inJail:true,
                    rollCount:1,
                    turnComplete:true
                })
                invoke.gameLog.create(gameState.gameId,`${player.playerName} went to jail.`)
            }
            //get out of jail free
            if(card.card.cardActionId === CardActionId.GetOutOfJailFree){
                invoke.player.update(player.id,{getOutOfJailFreeCards:player.getOutOfJailFreeCards + 1,turnComplete:true})
                invoke.gameLog.create(gameState.gameId,`${player.playerName} got a Get Out Of Jail Free card.`)
            }
            //Pay houses and hotel fees
            if(card.card.cardActionId === CardActionId.PayHouseHotel){
                invoke.player.update(player.id,{turnComplete:true})
                invoke.gameLog.create(gameState.gameId,`${player.playerName} had to pay money for their houses and hotels.`)
            }
            //get money from players
            if(card.card.cardActionId === CardActionId.ReceiveFromPlayers){
                const amount = card.card.amount || 0
                invoke.player.update(player.id,{turnComplete:true})
                gameState.players.forEach( (gamePlayer) => {
                    if(gamePlayer.id === player.id)return
                    invoke.player.update(gamePlayer.id,{money:gamePlayer.money - amount})
                })
                invoke.gameLog.create(gameState.gameId,`${player.playerName} took $${amount} from everyone.`)
            }
            //Advance to nearest railroad
            if(card.card.cardActionId === CardActionId.AdvanceToRailroad){
                const railroads = gameState.boardSpaces.filter( (space) => space.boardSpaceCategoryId === BoardSpaceCategory.Railroard)
                const closestRailroads = railroads.filter( (railroad) => railroad.id >= player.boardSpaceId)
                let closestRailroad;
                if(closestRailroads.length === 0){
                    closestRailroad = railroads[0]
                }else{
                    closestRailroad = closestRailroads[0]
                }

                invoke.player.update(player.id,{
                    boardSpaceId:closestRailroad.id,
                    ...(closestRailroad.id < player.boardSpaceId && {money: player.money + 200})
                })
                invoke.gameLog.create(gameState.gameId,`${player.playerName} went to the nearest railroad.`)
            }
            //Advance to nearest utility
            if(card.card.cardActionId === CardActionId.AdvanceToUtility){
                const utilities = gameState.boardSpaces.filter( (space) => space.boardSpaceCategoryId === BoardSpaceCategory.Utility)
                const closestUtilities = utilities.filter( (utility) => utility.id >= player.boardSpaceId)
                let closestUtility;
                if(closestUtilities.length === 0){
                    closestUtility = utilities[0]
                }else{
                    closestUtility = closestUtilities[0]
                }

                invoke.player.update(player.id,{
                    boardSpaceId:closestUtility.id,
                    ...(closestUtility.id < player.boardSpaceId && {money: player.money + 200})
                })
                invoke.gameLog.create(gameState.gameId,`${player.playerName} went to the nearest utility.`)
            }
            //pay players
            if(card.card.cardActionId === CardActionId.PayPlayers){
                const amount = card.card.amount || 0
                let playerMoney = player.money
                invoke.player.update(player.id,{turnComplete:true})
                gameState.players.forEach( (gamePlayer) => {
                    if(gamePlayer.id === player.id)return
                    playerMoney -= amount;
                    invoke.player.update(gamePlayer.id,{money:gamePlayer.money + amount})
                })
                invoke.player.update(player.id,{money:playerMoney})
                invoke.gameLog.create(gameState.gameId,`${player.playerName} paid $${amount} to everyone.`)
            }
            
            //TODO: this needs to be sent to all players, not just player who landed on it
            gameDispatch({cardToastMessage:card.cardDescription})
        })()
    }
    //=====================
    // Landed On Jail
    //=====================
    if(currentBoardSpace.boardSpaceCategoryId === BoardSpaceCategory.Jail){
        invoke.player.update(player.id,{turnComplete:true})
    }
    setLastBoardSpace(currentBoardSpace)
}