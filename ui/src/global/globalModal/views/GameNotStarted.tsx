import { PlayerInfoForm } from "../../PlayerInfoForm";

export const GameNotStarted = () => {

    return (
        <div className='relative flex flex-col gap-[20px]'>
                <div className='rounded-[20px] bg-white p-[10px] border border-black'>
                <h1 className='font-totoro text-[2rem] text-center'>Welcome to YamaTotoro Monopoly!</h1>

                    <p className='text-center'>Select a nickname and a player icon, then hit ready to enter the lobby.</p>
                </div>
                <PlayerInfoForm/>
        </div>
    )
    
}   