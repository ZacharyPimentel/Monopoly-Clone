/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { PlayerDebt } from "./PlayerDebt";

export interface Player {
    id: string;
    playerName: string;
    iconId: number;
    active: boolean;
    money: number;
    boardSpaceId: number;
    previousBoardSpaceId: number;
    isReadyToPlay: boolean;
    inCurrentGame: boolean;
    rollCount: number;
    canRoll: boolean;
    inJail: boolean;
    gameId: string;
    rollingForUtilities: boolean;
    jailTurnCount: number;
    getOutOfJailFreeCards: number;
    bankrupt: boolean;
    iconUrl: string;
    iconName: string;
    debts: PlayerDebt[];
    turnOrder: number;
}
