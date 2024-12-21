export type PlayerOfferCreateParams = {
    playerId:string
    initiator:boolean
    money:number
    getOutOfJailFreeCards:number
    gamePropertyIds: number[]
}

export type Trade = {
    id:number
    playerTrades:{
        getOutOfJailFreeCards:number
        money:number
        playerId:string
        initiator:boolean
        tradeProperties:{
            mortgaged:boolean
            propertyName:string
            gamePropertyId:number
        }[]
    }[]
}