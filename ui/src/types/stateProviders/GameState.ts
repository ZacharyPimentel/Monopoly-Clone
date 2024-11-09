import { ReactNode } from "react"
import { Game } from "../controllers/Game"
import { Player } from "../controllers/Player"
import { BoardSpace } from "../controllers/BoardSpace"

export type GameState = {
    ws: signalR.HubConnection
    players:Player[]
    currentSocketPlayer: {playerId:string,socketId:string} | null
    modalOpen:boolean
    modalContent:ReactNode | null
    theme:Theme
    boardRotation: 0 | 90 | 180 | 270
    gameState:Game | null,
    lastDiceRoll:number[] | null
    rolling:boolean
    boardSpaces:BoardSpace[]
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