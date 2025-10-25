import { useState } from "react"
import { ActionButtons } from "@globalComponents";
import { useWebSocket, useApi } from "@hooks";
import { OptionSelectMenu } from "@globalComponents";

export const GameCreateModal = () => {

    const [gameName,setGameName] = useState('');
    const {invoke} = useWebSocket();
    const api = useApi();
    const [themeId,setThemeId] = useState('1');
    const [passwordEnabled,setPasswordEnabled] = useState(false);
    const [password,setPassword] = useState('');

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
            <label className='flex flex-wrap gap-[10px] items-center'>
                <p className='min-w-[165px]'>Password Enabled</p>
                <input checked={passwordEnabled} onChange={(e) => {
                    if(e.target.checked){
                        setPasswordEnabled(true)
                    }else{
                        setPasswordEnabled(false)
                        return
                    }
                }} type='checkbox' className='scale-[1.5]'/>
            </label>

            {passwordEnabled && (
                <label className='flex flex-wrap gap-[10px] items-center'>
                    <p className='required min-w-[165px]'>Password:</p>
                    <input value={password} onChange={(e) => setPassword(e.target.value)} className='text-input flex-1' type='text'/>
                </label>
            )}
            
            <ActionButtons
                confirmCallback={async() => {
                    invoke.game.create({
                        gameCreateParams:{
                            gameName,
                            themeId: parseInt(themeId)},
                        password
                    });
                }}
                confirmButtonStyle="success"
                confirmButtonText="Create"
                confirmDisabled={
                    gameName === '' || 
                    themeId === '' ||
                    (passwordEnabled && password == '')
                }
            />
        </div>
    )
}