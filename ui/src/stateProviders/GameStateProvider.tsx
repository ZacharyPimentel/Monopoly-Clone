import { createContext, useCallback, useContext,useEffect,useState } from 'react';
import { GameState } from '../types/GameState';
import { RichUpTheme } from '../themes/RichUpTheme';
import { Game } from '../types/controllers/Game';
import { LoadingSpinner } from '../global/LoadingSpinner';
import { useGlobalDispatch } from './GlobalStateProvider';
import { PlayerCreateModal } from '../global/GlobalModal/modalContent/PlayerCreateModal';
import { Player } from '../types/controllers/Player';

const GameStateContext = createContext<any | null>(null);
const GameDispatchContext = createContext<(newState:Partial<GameState>) => void>(() => {});

export const GameStateProvider:React.FC<{children:React.ReactNode}> = ({ children }) => {

  const globalDispatch = useGlobalDispatch();

  const initialGameState:GameState = {
    //@ts-ignore
    ws: window.socketConnection,
    players:[],
    currentPlayer:null,
    gameInProgress:false,
    theme: RichUpTheme,
    boardRotation:90,
    gameState:null,
    currentSocketPlayer:null
  }

  const [gameState, setGameState] = useState<GameState>(initialGameState)
  const updateGameState = useCallback((newGameState:Partial<GameState>) => {
    setGameState( (prevState) => {
        return {...prevState, ...newGameState}
    })
  },[])

  useEffect( () => {
      if(!gameState.ws)return

      gameState.ws.on("OnConnectGameState", (gameState:Game) => {
          console.log('gameState',gameState)
          updateGameState({gameState})
      });

      gameState.ws.on("UpdateCurrentPlayer", (currentSocketPlayer) => {
        console.log('currentPlayer',currentSocketPlayer)
          updateGameState({currentSocketPlayer})
      });       
      gameState.ws.on("UpdatePlayers", (players:Player[]) => {
        console.log("UpdateCurrentPlayers: ", players)
          updateGameState({players})
      });
  },[]);

  useEffect( () => {
    if(!gameState.currentSocketPlayer) return

    if(!gameState.currentSocketPlayer.playerId){
      globalDispatch({modalOpen:true,modalContent:<PlayerCreateModal/>})
    }
  },[gameState.currentSocketPlayer])

  //only let people through once they've connected to the socket
  if(!gameState || !gameState.currentSocketPlayer){
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