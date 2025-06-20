import { useGameState } from "../../../../../../stateProviders/GameStateProvider"

export const GameLogs = () => {
    const {gameLogs} = useGameState();

    return (
        <ul className='flex flex-col w-full'>
            {gameLogs.map( (log,index) => {
                const opacity = 1 / (index + 1)
                return (
                    <li 
                        style={{opacity}}
                        key={log.id}
                        className="text-white transition-[0.2s] text-center px-[10px]"
                    >
                        {log.message}
                    </li>
                )
            })}
        </ul>
    )
}