import { GameLog } from "@generated/index";
import { useCallbackQueue } from "@hooks/useCallbackQueue";
import { useGameState } from "@stateProviders/GameStateProvider"
import { useCallback, useEffect, useState } from "react";

export const GameLogs = () => {
    const {gameLogs} = useGameState();
    const [displayedLogs,setDisplayedLogs] = useState(gameLogs);

    const delay = 200;
    const {pushToQueue} = useCallbackQueue(delay);

    const gameLogScollCallback = useCallback( (currentGameLogs:GameLog[]) => {
        if(!currentGameLogs)return
        const displayedLogIds = displayedLogs.map( log => log.id);
        const newLogs = currentGameLogs.filter( (currentLog) => !displayedLogIds.includes(currentLog.id))
        newLogs.sort((a,b) => a.id - b.id);
        const newDisplayedLogs = [...displayedLogs];
        newLogs.forEach( (log) => {
            newDisplayedLogs.unshift(log)
        })
        setDisplayedLogs(newDisplayedLogs)
    },[displayedLogs])

    useEffect(() => {
        if(!gameLogs)return
        pushToQueue(() => gameLogScollCallback(gameLogs))
    },[gameLogs])

    return (
        <div className='flex flex-col-reverse w-full overflow-hidden h-[120px]'>
            <ul style={{transform:`translateY(${(displayedLogs.length - 5) * 24}px)`}} className="duration-500">
                {displayedLogs.map( (log,index) => {
                    const opacity = 1 / (index + 1);
                    return (
                        <li 
                        style={{opacity}}
                            key={log.id}
                            className="text-white transition duration-1000 text-center px-[10px]"
                        >
                            {log.message}
                        </li>
                    )
                })}
            </ul>
        </div>
    )
}