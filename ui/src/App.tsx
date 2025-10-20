import { Navigate, Route, Routes } from "react-router-dom"
import { GamePage } from "./pages/GamePage/GamePage"
import { GlobalModal } from "@globalComponents/GlobalModal"
import { LobbyPage } from "./pages/LobbyPage/LobbyPage"
import { ToastContainer } from "react-toastify"
import { useEffect } from "react"
import * as signalR from "@microsoft/signalr"
import { useGlobalState } from "@stateProviders"
import { LoadingSpinner } from "@globalComponents";

export const App = () => {

  const {ws, dispatch} = useGlobalState(['ws']);

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
        dispatch({ws:connection})
      })
  },[])
  
  if(!ws)return <div className='flex w-full h-full justify-center items-center'><LoadingSpinner/></div>

  return (
    <Routes>
      <Route path='/*' element={<>
        <GlobalModal/>
        <ToastContainer/>
          <Routes>
            <Route path='lobby' element={<>
              <LobbyPage/>
            </>}/>
            <Route path='game/:gameId' element={
              <GamePage/>
            }/>
            <Route path='*' element={<Navigate to='lobby'/>}/>
          </Routes>
       </>}>
      </Route>
    </Routes>
  )
}