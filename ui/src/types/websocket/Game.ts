export type LobbyGame = {
    id:number
    inLobby:boolean
    gameOver:boolean
    gameStarted:boolean
    startingMoney:number
    currentPlayerTurn:string | null
    gameName:string
    activePlayerCount:number
}

export type GameUpdateParams = {
    startingMoney?:number
    fullSetDoublePropertyRent?:boolean
}