import { BoardSpace, Game, GameLog, Player, SocketPlayer, Trade } from "@generated";
import {create} from "zustand";
import { useShallow } from "zustand/shallow";

// Types for the state
interface GameState {
    players:Player[]
    currentSocketPlayer: SocketPlayer | null
    boardRotation: 0 | 90 | 180 | 270
    game:Game | null,
    rolling:boolean
    boardSpaces:BoardSpace[]
    gameId:string
    gameLogs:GameLog[]
    cardToastMessage:string
    trades:Trade[],
    queueMessageCount:number,
    //remove this later
    theme: any
    dispatch: (partial: Partial<GameState>) => void;
}
type GameStateKeys = keyof Omit<GameState, 'dispatch'>;

// The initialized state
const useGameStore = create<GameState>((set) => ({
    players:[],
    theme: {},
    boardRotation:90,
    game:null,
    currentSocketPlayer:null,
    rolling:false,
    boardSpaces:[],
    gameId:'',
    gameLogs:[],
    cardToastMessage:'',
    trades:[],
    queueMessageCount:0,
    dispatch: (partial: Partial<GameState>) => set((state) => ({ ...state, ...partial })),
}));

// State getter and setter hook
export const useGameState = <StateKeys extends GameStateKeys[]>(keys: StateKeys) => {
  const state = useGameStore(
    useShallow(state => {
      const selectedState = keys.reduce((acc, key) => {
        (acc as any)[key] = state[key];
        return acc;
      }, {} as Pick<GameState, StateKeys[number]>);
      return selectedState;
    }));

  const dispatch = useGameStore((state) => state.dispatch);
   
  return {
      ...state,
      dispatch
  } as
    { [K in StateKeys[number]]: typeof state[K]; } &
    { dispatch: (partial: Partial<GameState>) => void };
};