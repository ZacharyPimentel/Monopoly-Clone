/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { PropertyRent } from "./PropertyRent";

export interface Property {
    id: number;
    purchasePrice: number;
    mortgageValue: number;
    upgradeCost: number;
    boardSpaceId: number;
    setNumber?: number;
    propertyRents: PropertyRent[];
    gamePropertyId?: number;
    playerId?: string;
    upgradeCount: number;
    mortgaged?: boolean;
    gameId?: string;
    color?: string;
}
