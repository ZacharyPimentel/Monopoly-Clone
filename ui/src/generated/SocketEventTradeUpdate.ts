/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { PlayerTradeOffer } from "./PlayerTradeOffer";

export interface SocketEventTradeUpdate {
    tradeId: number;
    playerOne: PlayerTradeOffer;
    playerTwo: PlayerTradeOffer;
}
