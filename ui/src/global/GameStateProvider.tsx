import { createContext, useCallback, useContext,useEffect,useState } from 'react';
import { GameState, Player } from '../types/GameState';
import { LoadingSpinner } from './LoadingSpinner';
import { GameStarted } from './globalModal/views/GameStarted';
import { GameNotStarted } from './globalModal/views/GameNotStarted';
import { RichUpTheme } from '../themes/RichUpTheme';

const GameStateContext = createContext<any | null>(null);
const GameDispatchContext = createContext<(newState:Partial<GameState>) => void>(() => {});

export const GameStateProvider:React.FC<{children:React.ReactNode}> = ({ children }) => {

  const initialGameState:GameState = {
    //@ts-ignore
    ws: window.socketConnection,
    players:[],
    currentPlayer:null,
    gameInProgress:false,
    modalOpen:true,
    modalContent: null,
    theme: RichUpTheme,
    boardRotation:90,
  }

  const [gameState, setGameState] = useState<GameState>(initialGameState)
  const updateGameState = useCallback((newGameState:Partial<GameState>) => {
    setGameState( (prevState) => {
        return {...prevState, ...newGameState}
    })
  },[])

  useEffect( () => {
    gameState.ws.on("UpdateGameInProgress", (newValue:boolean) => {
      console.log("UpdateGameInProgress: ", newValue)
      updateGameState({gameInProgress:newValue});
      if(newValue === true){
          updateGameState({modalContent: <GameStarted/>})
      }else{
        updateGameState({modalContent: <GameNotStarted/>})
      }
    })
    gameState.ws.on("UpdateCurrentPlayer", (currentPlayer:Player) => {
      console.log("UpdateCurrentPlayer: ", currentPlayer)
        updateGameState({currentPlayer})
    });       
    gameState.ws.on("UpdateCurrentPlayers", (players:Player[]) => {
      console.log("UpdateCurrentPlayers: ", players)
        updateGameState({players})
    });
},[]);

  //only let people through once they've connected to the socket
  if(!gameState || !gameState.currentPlayer?.id){
    return <div className='flex justify-center items-center h-full w-full'><LoadingSpinner/></div>
  }

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