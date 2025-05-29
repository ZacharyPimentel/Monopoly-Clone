/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { TradeProperty } from "./TradeProperty";

export interface PlayerTrade {
    id: number;
    tradeId: number;
    playerId: string;
    money: number;
    getOutOfJailFreeCards: number;
    tradeProperties: TradeProperty[];
}
