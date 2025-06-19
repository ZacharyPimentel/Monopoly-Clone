import { createContext, useCallback, useContext,useState } from 'react';
import { GameState } from '../types/stateProviders/GameState';
import { RichUpTheme } from '../themes/RichUpTheme';

const GameStateContext = createContext<any | null>(null);
const GameDispatchContext = createContext<(newState:Partial<GameState>) => void>(() => {});

export const GameStateProvider:React.FC<{children:React.ReactNode}> = ({ children }) => {

  const initialGameState:GameState = {
    players:[],
    currentPlayer:null,
    gameInProgress:false,
    theme: RichUpTheme,
    boardRotation:90,
    game:null,
    currentSocketPlayer:null,
    rolling:false,
    boardSpaces:[],
    gameId:'',
    gameLogs:[],
    cardToastMessage:'',
    trades:[],
    queueMessageCount:0
  }

  const [gameState, setGameState] = useState<GameState>(initialGameState)
  const updateGameState = useCallback((newGameState:Partial<GameState>) => {
    setGameState( (prevState) => {
        return {...prevState, ...newGameState}
    })
  },[])

  

  return (
    <GameStateContext.Provider value={gameState}>
      <GameDispatchContext.Provider value={updateGameState}>
        {children}
      </GameDispatchContext.Provider>
    </GameStateContext.Provider>
  );
}

export function useGameState():GameState {
  return useContext(GameStateContext);
}

export function useGameDispatch():React.Dispatch<Partial<GameState>>{
  return useContext(GameDispatchContext);
}