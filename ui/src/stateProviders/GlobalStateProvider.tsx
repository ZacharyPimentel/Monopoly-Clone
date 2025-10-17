import { createContext, useCallback, useContext,useEffect,useState } from 'react';
import { GlobalState } from '../types/stateProviders/GlobalState';
import { LoadingSpinner } from '../globalComponents/LoadingSpinner';
import * as signalR from '@microsoft/signalr';

const GlobalStateContext = createContext<any | null>(null);
const GlobalDispatchContext = createContext<(newState:Partial<GlobalState>) => void>(() => {});

export const GlobalStateProvider:React.FC<{children:React.ReactNode}> = ({ children }) => {

  const initialGlobalState:GlobalState = {
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
    availableGames:[]
  }

  const [globalState, setglobalState] = useState<GlobalState>(initialGlobalState)
  const updateGlobalState = useCallback((newglobalState:Partial<GlobalState>) => {
    setglobalState( (prevState) => {
        return {...prevState, ...newglobalState}
    })
  },[])

  //set up the web socket connection
  useEffect( () => {
    const connection = new signalR.HubConnectionBuilder()
    .withUrl(import.meta.env.VITE_SOCKET_URL,{
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets
    })
    .build();
    connection.start()
      .then( () => {
        updateGlobalState({ws:connection})
      })
  },[])

  //wait for the socket connection to establish before loading the app
  if(!globalState.ws)return <div className='flex w-full h-full justify-center items-center'><LoadingSpinner/></div>

  return (
    <GlobalStateContext.Provider value={globalState}>
      <GlobalDispatchContext.Provider value={updateGlobalState}>
        {children}
      </GlobalDispatchContext.Provider>
    </GlobalStateContext.Provider>
  );
}

export function useGlobalState():GlobalState {
  return useContext(GlobalStateContext);
}

export function useGlobalDispatch():React.Dispatch<Partial<GlobalState>>{
  return useContext(GlobalDispatchContext);
}

export function useGlobalContext():{
  globalState:GlobalState,
  globalDispatch:React.Dispatch<Partial<GlobalState>>
}{
  return {
    globalState: useContext(GlobalStateContext),
    globalDispatch: useContext(GlobalDispatchContext)
  }
}