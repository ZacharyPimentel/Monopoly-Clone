import React, { useState } from "react";
import { ActionButtons } from "../ActionButtons";
import { useWebSocket } from "@hooks/useWebSocket";

export const EnterPasswordModal:React.FC<{gameId:string}> = ({gameId}) => {

    const {invoke} = useWebSocket();
    const [password,setPassword] = useState('')

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Enter Password</p>
            <p>Please enter the password to join this game.</p>
            <label className='flex flex-wrap gap-[10px] items-center'>
                <p className='required min-w-[165px]'>Password:</p>
                <input value={password} onChange={(e) => setPassword(e.target.value)} className='text-input flex-1' type='text'/>
            </label>
            <ActionButtons 
                confirmCallback={async() => {
                    invoke.game.validatePassword({password,gameId});
                }}
                confirmButtonStyle={"success"} 
                confirmButtonText={"Join"}
                confirmDisabled={password === ''}
            />
        </div>
    )
}