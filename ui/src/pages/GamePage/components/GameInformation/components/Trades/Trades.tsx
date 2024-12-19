import { useGlobalDispatch } from "../../../../../../stateProviders/GlobalStateProvider"
import { CreateTradeModal } from "./modal/CreateTradeModal";

export const Trades = () => {

    const globalDispatch = useGlobalDispatch();

    return (
        <div className='flex flex-col gap-[10px] p-[30px] bg-totorogreen w-full'>
            <div className='flex justify-between gap-[20px]'>
                <p>Trades</p>
                <button onClick={() => globalDispatch({modalOpen:true,modalContent:<CreateTradeModal/>})}>
                    <svg className='fill-black' height="24px" viewBox="0 -960 960 960" width="24px" fill="#5f6368"><path d="M440-440H200v-80h240v-240h80v240h240v80H520v240h-80v-240Z"/></svg>
                </button>
            </div>
        </div>
    )
}