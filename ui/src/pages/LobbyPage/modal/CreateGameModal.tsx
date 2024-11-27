import { useState } from "react"

import { ActionButtons } from "../../../globalComponents/GlobalModal/ActionButtons";
import { useWebSocket } from "../../../hooks/useWebSocket";
import { OptionSelectMenu } from "../../../globalComponents/FormElements/OptionSelectMenu";
import { useApi } from "../../../hooks/useApi";

export const GameCreateModal = () => {

    const [gameName,setGameName] = useState('');
    const {invoke} = useWebSocket();
    const api = useApi();
    const [themeId,setThemeId] = useState('1');

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Create New Game</p>
            <label className='flex flex-wrap gap-[10px] items-center'>
                <p className='required min-w-[165px]'>Game Name:</p>
                <input value={gameName} onChange={(e) => setGameName(e.target.value)} className='text-input flex-1' type='text'/>
            </label>
            <label className='flex flex-wrap gap-[10px] items-center'>
            <p className='required min-w-[165px]'>Theme:</p>
            <OptionSelectMenu
                defaultValue="1"
                displayKey="themeName"
                apiCall={async() => {
                    const themes = await api.theme.getAll();
                    return themes;
                }}
                setStateCallback={(newValue) => setThemeId(newValue)}
            />
            </label>
            
            <ActionButtons
                confirmCallback={async() => {
                    invoke.game.create(gameName,parseInt(themeId));
                }}
                confirmButtonStyle="success"
                confirmButtonText="Create"
                confirmDisabled={gameName === '' || themeId === ''}
            />
        </div>
    )
}