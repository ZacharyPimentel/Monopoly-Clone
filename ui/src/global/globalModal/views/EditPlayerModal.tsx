import { useState } from "react"
import { PlayerInfoForm } from "../../PlayerInfoForm"
import { useGameState } from "../../../stateProviders/GameStateProvider"

export const EditPlayerModal = () => {
    const gameState = useGameState()
    const [nameInput,setNameInput] = useState(gameState.currentPlayer?.nickName || '')

    return (
        <div className='flex flex-col gap-[20px]'>
            <div className="bg-white rounded-[20px] p-[10px]">
                <p className='text-[2rem] font-totoro text-center'>Customize My Player</p>
            </div>
            <p className='text-center font-bold'>Enter a new nickname</p>
            <PlayerInfoForm/>
        </div>
    )
}