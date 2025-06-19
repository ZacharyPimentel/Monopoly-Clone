import { useEffect, useState } from "react"
import { useApi } from "@hooks/useApi";
import { useGameState } from "@stateProviders/GameStateProvider";
import { GameLog } from "src/types/controllers/GameLog";

export const ViewLogsModal = () => {

    const api = useApi();
    const gameState = useGameState();
    const [logs,setLogs] = useState<GameLog[]>([]);

    useEffect( () => {
        (async() => {
            if(!gameState?.game)return
            const logs = await api.gameLog.getAll(gameState.game.id)
            setLogs(logs);
        })()
    },[])

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Logs</p>
            <ul className='max-h-[300px] overflow-y-scroll flex flex-col gap-[10px]'>
                {logs.reverse().map( log => {
                    return (
                        <li key={log.id} className='flex flex-col'>
                            <p className='opacity-[0.5] text-[12px]'>{new Date(log.createdAt).toTimeString().slice(0, 8)}</p>
                            {log.message}
                        </li>
                    )
                })}
            </ul>
        </div>
    )
}