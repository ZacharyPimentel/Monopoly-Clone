import { Route, Routes } from "react-router-dom"
import { GameStateProvider } from "./stateProviders/GameStateProvider"
import { GamePage } from "./pages/GamePage/GamePage"
import { GlobalModal } from "./globalComponents/GlobalModal/GlobalModal"
import { GlobalStateProvider } from "./stateProviders/GlobalStateProvider"

export const App = () => {
  return (
    <Routes>
      <Route path='/' element={
        <GlobalStateProvider>
          <GameStateProvider>
            <GlobalModal/>
            <GamePage/>
          </GameStateProvider>
        </GlobalStateProvider>
      }>
      </Route>
    </Routes>
  )
}

