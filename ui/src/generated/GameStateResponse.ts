/**
 * This is a TypeGen auto-generated file.
 * Any changes made to this file can be lost when this file is regenerated.
 */

import { Game } from "./Game";
import { Player } from "./Player";
import { GameLog } from "./GameLog";
import { BoardSpace } from "./BoardSpace";
import { Trade } from "./Trade";

export interface GameStateResponse {
    game?: Game;
    players?: Player[];
    gameLogs?: GameLog[];
    boardSpaces?: BoardSpace[];
    trades?: Trade[];
}
