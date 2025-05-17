import { ViewLogsModal } from "src/globalComponents/GlobalModal/modalContent/ViewLogsModal"
import { useGlobalDispatch } from "@stateProviders/GlobalStateProvider"

export const GameStatus = () => {
    
    const globalDispatch = useGlobalDispatch()

    return (
        <div className='flex flex-col gap-[10px] p-[30px] bg-totorogreen w-full'>
            Game status
            <ul className='flex flex-col gap-[10px]'>
                <button onClick={() => globalDispatch({modalOpen:true,modalContent:<ViewLogsModal/>})} className='btn-secondary'>View Game Logs</button>
            </ul>
        </div>
    )
}