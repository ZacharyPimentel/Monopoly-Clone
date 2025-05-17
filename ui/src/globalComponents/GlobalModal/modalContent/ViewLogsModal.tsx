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
            const logs = await api.gameLog.getAll(gameState.gameId)
            setLogs(logs);
        })()
    },[])

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Logs</p>
            <ul>
                {logs.map( log => {
                    return (
                        <li key={log.id}>{log.message}</li>
                    )
                })}
            </ul>
        </div>
    )
}