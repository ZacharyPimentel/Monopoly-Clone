import { useEffect, useRef, useState} from "react";
import { useGlobalDispatch, useGlobalState } from "../../stateProviders/GlobalStateProvider";
import { useLocation } from "react-router-dom";

export const GlobalModal = () => {
    const globalModalRef = useRef<HTMLDialogElement | null>(null);
    const globalState = useGlobalState();
    const globalDispatch = useGlobalDispatch()
    const location = useLocation();

    const [opacity,setOpacity] = useState(1);
    const [scale,setScale] = useState(0)

    const [lastcontent,setLastContent] = useState(globalState.modalContent);

    useEffect( () => {
        if(!globalModalRef.current)return;
        const timeout = setTimeout( () => {
            globalModalRef.current?.close();
        },300)
        if(globalState.modalOpen){
            setOpacity(1)
            setScale(1)
            setLastContent(globalState.modalContent)
            clearTimeout(timeout)
            globalModalRef.current.show()
        }else{
            setScale(0)
            setOpacity(0)
        }
        return () => {
            clearTimeout(timeout);
        }
    },[globalState.modalOpen,globalState.modalContent])

    //close the modal on route change
    useEffect( () => {
        return () => {
            globalDispatch({modalOpen:false,modalContent:null})
        }
    },[location])
    
    return (
        <dialog style={{opacity,transform:`scale(${scale}) translateY(-50%)`}} ref={globalModalRef} className='transition-all duration-[300ms] fixed shadow-lg top-[50%] w-[75%] max-w-[600px] z-[10] max-h-[75vh]'>
            <img className='absolute bottom-full top-[-70px] rotate-[-15deg] w-[100px]' src='/assets/totoro.png'/>
            <button onClick={() => {
                setOpacity(0)
                setScale(0)
                setTimeout( () => {
                    globalDispatch({modalOpen:false,modalContent:null})
                },300)}
             } className='absolute z-[1] top-[-14px] right-[20px] border-2 border-black rounded-[50%] bg-white hover:rotate-[90deg] transition-[0.2s]'>
                <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="m256-200-56-56 224-224-224-224 56-56 224 224 224-224 56 56-224 224 224 224-56 56-224-224-224 224Z"/></svg>
            </button>
            <div className='flex flex-col gap-[20px] p-[20px] relative bg-totorolightgreen border-2 border-black'>
                {globalState.modalOpen && lastcontent}
            </div>
        </dialog>
    )
}