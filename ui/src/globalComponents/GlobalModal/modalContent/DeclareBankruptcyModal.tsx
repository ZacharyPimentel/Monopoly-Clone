import { useWebSocket } from "@hooks/useWebSocket";
import { ActionButtons } from "../ActionButtons";

export const DeclareBankruptcyModal = () => {

    const {invoke} = useWebSocket();

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Declare Bankruptcy</p>
            <p>Are you ready to go bankrupt?</p>
            <ActionButtons 
                confirmCallback={async() => {
                    invoke.player.goBankrupt();
                }} 
                confirmButtonStyle={"warning"} 
                confirmButtonText={"Bankrupt"}
            />
        </div>
    )
}