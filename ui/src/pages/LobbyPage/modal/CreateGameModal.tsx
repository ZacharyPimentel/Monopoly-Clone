import { useMemo, useState } from "react"
import { useApi } from "../../../hooks/useApi"
import { useGameState } from "../../../stateProviders/GameStateProvider";
import { useGlobalDispatch } from "../../../stateProviders/GlobalStateProvider";
import { PlayerIcon } from "../../../types/controllers/PlayerIcon";
import { ActionButtons } from "../../../globalComponents/GlobalModal/ActionButtons";
import { useWebSocket } from "../../../hooks/useWebSocket";

export const GameCreateModal = () => {

    const [gameName,setGameName] = useState('');
    const {invoke} = useWebSocket();

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Create New Game</p>
            <label className='flex flex-wrap gap-[10px] items-center'>
                <p className='required min-w-[165px]'>Game Name:</p>
                <input value={gameName} onChange={(e) => setGameName(e.target.value)} className='text-input flex-1' type='text'/>
            </label>
            <ActionButtons
                confirmCallback={async() => {
                    invoke.game.create(gameName);
                }}
                confirmButtonStyle="success"
                confirmButtonText="Create"
                confirmDisabled={gameName === ''}
            />
        </div>
    )
}