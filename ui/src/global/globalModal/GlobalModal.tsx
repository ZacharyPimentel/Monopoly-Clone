import { useEffect, useRef } from "react";
import { useGlobalDispatch, useGlobalState } from "../../stateProviders/GlobalStateProvider";

export const GlobalModal = () => {
    const globalModalRef = useRef<HTMLDialogElement | null>(null);
    const globalState = useGlobalState();
    const globalDispatch = useGlobalDispatch()

    useEffect( () => {
        if(!globalModalRef.current)return;
        if(globalState.modalOpen){
            globalModalRef.current.show()
        }else{
            globalModalRef.current.close();
        }
    },[globalState.modalOpen])

    return (
        <dialog ref={globalModalRef} className='shadow-lg top-[50%] translate-y-[-50%] w-[75%] max-w-[600px] z-[10]'>
            <img className='absolute bottom-full top-[-70px] rotate-[-15deg] w-[100px]' src='/assets/totoro.png'/>
            <button onClick={() => globalDispatch({modalOpen:false,modalContent:null})} className='absolute z-[1] top-[-14px] right-[20px] border-2 border-black rounded-[50%] bg-white hover:rotate-[90deg] transition-[0.2s]'>
                <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="m256-200-56-56 224-224-224-224 56-56 224 224 224-224 56 56-224 224 224 224-56 56-224-224-224 224Z"/></svg>
            </button>
            <div className='flex flex-col gap-[20px] p-[20px] relative bg-totorolightgreen border-2 border-black'>
                {globalState?.modalContent}
            </div>
        </dialog>
    )
}