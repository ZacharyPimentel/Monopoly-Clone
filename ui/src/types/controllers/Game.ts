export type Game = {
    id:string
    inLobby:boolean
    gameOver:boolean
    gameStarted:boolean
    startingMoney:number
    fullSetDoublePropertyRent:boolean
    currentPlayerTurn:string | null
    gameName:string
    diceOne:number
    diceTwo:number
}