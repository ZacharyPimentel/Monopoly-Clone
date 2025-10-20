import { Game } from "@generated";
import { ReactNode } from "react";
import {create} from "zustand";
import { useShallow } from "zustand/shallow";

// Types for the state
interface GlobalState {
    modalOpen:boolean
    modalContent: ReactNode | null
    toastOpen:boolean,
    toastStyle:'success' | 'info' | 'error',
    toastMessages:string[],
    ws: signalR.HubConnection
    availableGames:Game[]
    socketConnected:boolean
    dispatch: (partial: Partial<GlobalState>) => void;
}
type GlobalStateKeys = keyof Omit<GlobalState, 'dispatch'>;

// The initialized state
const useGlobalStore = create<GlobalState>((set) => ({
    //state vars here
    //modal
    modalOpen:false,
    modalContent:null,
    //toast
    toastOpen:false,
    toastStyle:'success',
    toastMessages:[],
    //socket connection
    //@ts-ignore
    ws: window.socketConnection,
    socketConnected:false,
    availableGames:[],
    dispatch: (partial: Partial<GlobalState>) => set((state) => ({ ...state, ...partial })),
}));

// State getter and setter hook
export const useGlobalState = <StateKeys extends GlobalStateKeys[]>(keys: StateKeys) => {
  const state = useGlobalStore(
    useShallow(state => {
      const selectedState = keys.reduce((acc, key) => {
        (acc as any)[key] = state[key];
        return acc;
      }, {} as Pick<GlobalState, StateKeys[number]>);
      return selectedState;
    }));

  const dispatch = useGlobalStore((state) => state.dispatch);
   
  return {
      ...state,
      dispatch
  } as
    { [K in StateKeys[number]]: typeof state[K]; } &
    { dispatch: (partial: Partial<GlobalState>) => void };
};