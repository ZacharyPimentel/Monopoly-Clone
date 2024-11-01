export type Player = {
    id:string
    iconId:number
    playerName:string
    isReadyToPlay:boolean
}

export type PlayerWhereParams = {
    isActive:boolean,
}

export type PlayerUpdateParams = {
    iconId:number,
    playerName:string,
    isReadyToPlay:boolean
}