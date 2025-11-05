import { PlayerTradeOffer, Trade } from "@generated";
import { usePlayer } from "./usePlayer";
import { useForm } from "react-hook-form";
import { useGameState } from "@stateProviders";

type TradeInputs = {
    playerOne:PlayerTradeOffer
    playerTwo:PlayerTradeOffer
}

type UseTradeFormParams =
  | { trade: Trade; tradePartnerId?: never }
  | { trade?: never; tradePartnerId: string };

export const useTradeForm = ({trade, tradePartnerId}:UseTradeFormParams) => {

    const {player} = usePlayer();
    const {boardSpaces} = useGameState(['boardSpaces'])
    
    const currentPlayersTrade = trade?.playerTrades.find( pt => pt.playerId === player.id)
    let leftSidePlayerTrade;
    let rightSidePlayerTrade 


    if(currentPlayersTrade){
        leftSidePlayerTrade = currentPlayersTrade;
        rightSidePlayerTrade = trade?.playerTrades.find( pt => pt.playerId !== player.id)
    }else{
        leftSidePlayerTrade = trade?.playerTrades[0]
        rightSidePlayerTrade = trade?.playerTrades[1]
    }

    const leftSidePlayerPropertyIds = boardSpaces
        .filter( bs => bs?.property?.playerId == leftSidePlayerTrade?.playerId)
        .map( bs => bs?.property?.gamePropertyId)
    const rightSidePlayerPropertyIds = boardSpaces
        .filter( bs => bs?.property?.playerId == rightSidePlayerTrade?.playerId)
        .map( bs => bs?.property?.gamePropertyId)
    
    const defaultValues = {
        playerOne:{
            playerId: leftSidePlayerTrade?.playerId ?? player.id,
            money:  leftSidePlayerTrade?.money ?? 0,
            getOutOfJailFreeCards: leftSidePlayerTrade?.getOutOfJailFreeCards ?? 0,
            gamePropertyIds: leftSidePlayerTrade?.tradeProperties
                ?.map( (tp) => tp.gamePropertyId) 
                .filter( gpId => leftSidePlayerPropertyIds.includes(gpId))
                ?? []
        },
        playerTwo:{
            playerId: rightSidePlayerTrade?.playerId ?? tradePartnerId,
            money:  rightSidePlayerTrade?.money ?? 0,
            getOutOfJailFreeCards:  rightSidePlayerTrade?.getOutOfJailFreeCards ?? 0,
            gamePropertyIds: rightSidePlayerTrade?.tradeProperties
            ?.map( (tp) => tp.gamePropertyId) 
            .filter( gpId => rightSidePlayerPropertyIds.includes(gpId))
            ?? []
        },
    }


    const form = useForm<TradeInputs>({
        mode:'onBlur',
        defaultValues:defaultValues
    })

    return form
}