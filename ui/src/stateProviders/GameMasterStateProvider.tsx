import { createContext, useCallback, useContext,useState } from 'react';
import { GameMasterState } from '../types/stateProviders/GameMasterState';

const GameMasterStateContext = createContext<any | null>(null);
const GameMasterDispatchContext = createContext<(newState:Partial<GameMasterState>) => void>(() => {});

export const GameMasterStateProvider:React.FC<{children:React.ReactNode}> = ({ children }) => {

  const initialGameMasterState:GameMasterState = {
    forceLandedSpace:0,
    forceNextCardId:0,
    forceDiceOne:0,
    forceDiceTwo:0
  }

  const [gameMasterState, setGameMasterState] = useState<GameMasterState>(initialGameMasterState)
  const updateGameMasterState = useCallback((newGameMasterState:Partial<GameMasterState>) => {
    setGameMasterState( (prevState) => {
        return {...prevState, ...newGameMasterState}
    })
  },[])

  return (
    <GameMasterStateContext.Provider value={gameMasterState}>
      <GameMasterDispatchContext.Provider value={updateGameMasterState}>
        {children}
      </GameMasterDispatchContext.Provider>
    </GameMasterStateContext.Provider>
  );
}

export function useGameMasterState():GameMasterState {
  return useContext(GameMasterStateContext);
}

export function useGameMasterDispatch():React.Dispatch<Partial<GameMasterState>>{
  return useContext(GameMasterDispatchContext);
}