export type Player = {
    id:string
    iconId:number
    iconUrl:string
    playerName:string
    isReadyToPlay:boolean
    active:boolean
    boardSpaceId:number
    money:number
    rollCount:number
    turnComplete:boolean
    inJail:boolean
    rollingForUtilities:boolean
    jailTurnCount:number
    getOutOfJailFreeCards:number
}

export type PlayerWhereParams = {
    isActive:boolean,
}

export type PlayerUpdateParams = {
    iconId:number,
    playerName:string,
    isReadyToPlay:boolean
    boardSpaceId:number
    rollCount:number
    money:number
    turnComplete:boolean
    inJail:boolean
    rollingForUtilities:boolean
    jailTurnCount:number
    getOutOfJailFreeCards:number
}