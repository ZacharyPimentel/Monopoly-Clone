import React, { useMemo } from "react";
import { ActionButtons } from "../ActionButtons";
import { useGameState } from "@stateProviders/GameStateProvider";
import { useNavigate } from "react-router-dom";

export const GameOverModal:React.FC<{}> = ({}) => {

    const {players} = useGameState();
    const navigate = useNavigate();

    const gameWinner = useMemo( () => {
        return players.find( player => !player.bankrupt);
    },[])

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Game Over</p>
            <div className="flex gap-[20px] items-center">
                <img className='w-[50px] h-[50px]' src={gameWinner?.iconUrl}/>
                <p>{gameWinner?.playerName} is the winner!</p>
            </div>
            
            <ActionButtons 
                confirmCallback={async() => {
                    navigate('/lobby')
                }} 
                confirmButtonStyle={"success"} 
                confirmButtonText={"Back To Lobby"}
            />
        </div>
    )
}