import { Navigate, Route, Routes } from "react-router-dom"
import { GamePage } from "./pages/GamePage/GamePage"
import { GlobalModal } from "@globalComponents"
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
    .withAutomaticReconnect()
    .build();
    connection.start()
      .then( () => {
        dispatch({ws:connection})
      })
    connection.onclose(error => {
        console.log("SignalR disconnected:", error);
        alert("You've been disconnected, please reload your page and reconnect.")
    });
  },[])
  
  if(!ws)return <div className='flex w-full h-full justify-center items-center'><LoadingSpinner/></div>

  return (
    <Routes>
      <Route path='/*' element={<>
        <ToastContainer/>
          <Routes>
            <Route path='lobby' element={<>
              <GlobalModal/>
              <LobbyPage/>
            </>}/>
            <Route path='game/:gameId' element={<>
              <div className='lg:hidden'>
                <GlobalModal/>
              </div>
              <GamePage/>
            </>}/>
            <Route path='*' element={<Navigate to='lobby'/>}/>
          </Routes>
       </>}>
      </Route>
    </Routes>
  )
}