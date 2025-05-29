/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { PlayerTrade } from "./PlayerTrade";

export interface Trade {
    id: number;
    gameId: string;
    initiatedBy: string;
    lastUpdatedBy: string;
    declinedBy?: string;
    acceptedBy?: string;
    playerTrades: PlayerTrade[];
}
