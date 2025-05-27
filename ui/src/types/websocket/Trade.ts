export type PlayerOfferCreateParams = {
    playerId:string
    money:number
    getOutOfJailFreeCards:number
    gamePropertyIds: number[]
}

export type Trade = {
    id:number
    lastUpdatedBy:string
    initiatedBy:string
    playerTrades:{
        getOutOfJailFreeCards:number
        money:number
        playerId:string
        tradeProperties:{
            mortgaged:boolean
            propertyName:string
            gamePropertyId:number
        }[]
    }[]
}