import { GameLog } from "@generated";
import { useCallbackQueue } from "@hooks";
import { useGameState } from "@stateProviders";
import { useCallback, useEffect, useState } from "react";
import useWindowSize from "src/hooks/useWindowSize";

export const GameLogs = () => {
    const {gameLogs} = useGameState(['gameLogs']);
    const [displayedLogs,setDisplayedLogs] = useState(gameLogs);

    const {width} = useWindowSize();

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

    
    const sizeFactor = width < 768 ? 12 : 24

    return (
        <div style={{height:`${sizeFactor * 5}px`}} className='flex flex-col-reverse w-full overflow-hidden'>
            <ul style={{transform:`translateY(${(displayedLogs.length - 5) * sizeFactor}px)`}} className="duration-500">
                {displayedLogs.map( (log,index) => {
                    const opacity = 1 / (index + 1);
                    return (
                        <li 
                        style={{opacity}}
                            key={log.id}
                            title={log.message}
                            className="text-white transition duration-1000 text-center text-[8px] md:text-[16px] px-[10px] truncate"
                        >
                            {log.message}
                        </li>
                    )
                })}
            </ul>
        </div>
    )
}