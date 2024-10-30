import { useEffect, useRef } from "react";
import { useGameDispatch, useGameState } from "../GameStateProvider"

export const GlobalModal = () => {
    const gameState = useGameState();
    const gameDispatch = useGameDispatch();
    const globalModalRef = useRef<HTMLDialogElement | null>(null);

    useEffect( () => {
        if(!globalModalRef.current)return;
        if(gameState.modalOpen){
            globalModalRef.current.show()
        }else{
            globalModalRef.current.close();
        }
    },[gameState?.modalOpen])

    return (
        <dialog ref={globalModalRef} className='shadow-lg top-[50%] translate-y-[-50%] w-[75%] max-w-[600px] z-[10]'>
            <img className='absolute bottom-full top-[-70px] rotate-[-15deg] w-[100px]' src='/assets/totoro.png'/>
            <button onClick={() => gameDispatch({modalOpen:false,modalContent:null})} className='absolute z-[1] top-[-14px] right-[20px] border-2 border-black rounded-[50%] bg-white hover:rotate-[90deg] transition-[0.2s]'>
                <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="m256-200-56-56 224-224-224-224 56-56 224 224 224-224 56 56-224 224 224 224-56 56-224-224-224 224Z"/></svg>
            </button>
            <div className='flex flex-col gap-[20px] p-[20px] relative bg-totorolightgreen border-2 border-black'>
                {gameState?.modalContent}
            </div>
        </dialog>
    )
}