import { ReactNode } from "react"
import { Player, BoardSpace, GameLog, SocketPlayer } from "@generated/index";
import { Trade } from "../websocket/Trade"
import { Game } from "@generated/Game"

export type GameState = {
    players:Player[]
    currentSocketPlayer: SocketPlayer | null
    modalOpen:boolean
    modalContent:ReactNode | null
    theme:Theme
    boardRotation: 0 | 90 | 180 | 270
    game:Game | null,
    lastDiceRoll:number[] | null
    rolling:boolean
    boardSpaces:BoardSpace[]
    gameId:string
    gameLogs:GameLog[]
    cardToastMessage:string
    trades:Trade[],
    queueMessageCount:number
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