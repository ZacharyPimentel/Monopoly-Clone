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