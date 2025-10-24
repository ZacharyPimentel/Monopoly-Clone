import { useGlobalState } from "@stateProviders";
import { ViewLogsModal } from "@globalComponents"

export const GameStatus = () => {
    
    const {dispatch:globalDispatch} = useGlobalState([]);

    return (
        <div className='flex flex-col gap-[10px] p-[30px] bg-totorogreen w-full'>
            Game status
            <ul className='flex flex-col gap-[10px]'>
                <button onClick={() => globalDispatch({modalOpen:true,modalContent:<ViewLogsModal/>})} className='btn-secondary'>View Game Logs</button>
            </ul>
        </div>
    )
}