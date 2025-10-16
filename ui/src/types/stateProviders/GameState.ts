import { Player, BoardSpace, GameLog, SocketPlayer } from "@generated/index";
import { Trade } from "../websocket/Trade"
import { Game } from "@generated/Game"

export type GameState = {
    players:Player[]
    currentSocketPlayer: SocketPlayer | null
    boardRotation: 0 | 90 | 180 | 270
    game:Game | null,
    rolling:boolean
    boardSpaces:BoardSpace[]
    gameId:string
    gameLogs:GameLog[]
    cardToastMessage:string
    trades:Trade[],
    queueMessageCount:number,
    //remove this later
    theme: any
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