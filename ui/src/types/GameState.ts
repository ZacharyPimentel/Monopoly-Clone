import { ReactNode } from "react"

export type GameState = {
    ws: signalR.HubConnection
    players:Player[]
    currentPlayer: Player | null
    gameInProgress:boolean
    modalOpen:boolean
    modalContent:ReactNode | null
    theme:Theme
    boardRotation: 0 | 90 | 180 | 270
}

export type Player = {
    id:string,
    isActive:boolean
    isCurrentGameParticipant:boolean
    isMyTurn:boolean
    nickName:string | null
    positionOnBoard:number
    gamePieceID:number
    gamePieceURL:string
    isConnected:boolean
    money:number
    isReady:boolean
}

export type Theme = {
    playerIcons:{
        id:number
        url:string
    }[]
    railroads:{
        id:number
        name:string
        purchasePrise:number
    }[]
    propertySets:{
        setId:number
        properties:{
            name:string
            values:{
                purchasePrice:number,
                mortgageValue:number,
                houseCost:number,
                baseRent:number,
                oneHouseRent: number,
                twoHouseRent: number,
                threeHouseRent:number,
                fourHouseRent:number,
                fiveHouseRent:number
            }
        }[]
    }[]
}