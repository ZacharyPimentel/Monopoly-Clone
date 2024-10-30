import { Route, Routes } from "react-router-dom"
import { GameStateProvider } from "./global/GameStateProvider"
import { GamePage } from "./pages/GamePage/GamePage"
import { GlobalModal } from "./global/globalModal/GlobalModal"

export const App = () => {
  return (
    <Routes>
      <Route path='/' element={
        
        <GameStateProvider>
          <GlobalModal/>
          <GamePage/>
        </GameStateProvider>
      }>
      </Route>
    </Routes>
  )
}

