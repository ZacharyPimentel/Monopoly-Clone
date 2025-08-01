import { Navigate, Route, Routes } from "react-router-dom"
import { GameStateProvider } from "./stateProviders/GameStateProvider"
import { GamePage } from "./pages/GamePage/GamePage"
import { GlobalModal } from "./globalComponents/GlobalModal/GlobalModal"
import { GlobalStateProvider } from "./stateProviders/GlobalStateProvider"
import { LobbyPage } from "./pages/LobbyPage/LobbyPage"
import { ToastContainer } from "react-toastify"

export const App = () => {
  return (
    <Routes>
      <Route path='/*' element={
        <GlobalStateProvider>
          <GameStateProvider>
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
          </GameStateProvider>
        </GlobalStateProvider>
      }>
      </Route>
    </Routes>
  )
}