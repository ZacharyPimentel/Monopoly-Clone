import { useState } from "react";
import { useGameState } from "../../global/GameStateProvider"
import { GameBoard } from "./components/gameBoard/GameBoard";
import { Navbar } from "./components/navbar/Navbar";
import { PlayerList } from "./components/playerList/PlayerList";

export const GamePage = () => {

    const gameState = useGameState();

    const [nameInput,setNameInput] = useState('');

    return (
        <div className='flex flex-col'>
            <Navbar/>
            <GameBoard/>
        </div>
    )
}