/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

export interface Player {
    id: string;
    playerName: string;
    iconId: number;
    active: boolean;
    money: number;
    boardSpaceId: number;
    isReadyToPlay: boolean;
    inCurrentGame: boolean;
    rollCount: number;
    turnComplete: boolean;
    inJail: boolean;
    gameId: string;
    rollingForUtilities: boolean;
    jailTurnCount: number;
    getOutOfJailFreeCards: number;
    iconUrl: string;
}
