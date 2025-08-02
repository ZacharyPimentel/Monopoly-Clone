/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

export interface Game {
    id: string;
    gameName: string;
    inLobby: boolean;
    gameOver: boolean;
    gameStarted: boolean;
    startingMoney: number;
    themeId: number;
    fullSetDoublePropertyRent: boolean;
    extraMoneyForLandingOnGo: boolean;
    collectMoneyFromFreeParking: boolean;
    moneyInFreeParking: number;
    currentPlayerTurn?: string;
    hasPassword?: boolean;
    activePlayerCount?: number;
    diceRollInProgress: boolean;
    movementInProgress: boolean;
    diceOne?: number;
    diceTwo?: number;
    utilityDiceOne?: number;
    utilityDiceTwo?: number;
}
