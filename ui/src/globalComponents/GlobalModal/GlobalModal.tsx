import { useEffect, useRef, useState} from "react";
import { useLocation } from "react-router-dom";
import { useCallbackQueue } from "@hooks";
import { useGlobalState } from "@stateProviders";
import { Minus, Triangle } from "lucide-react";

export const GlobalModal = () => {
    const globalModalRef = useRef<HTMLDialogElement | null>(null);
    const {dispatch, ...globalState} = useGlobalState(['modalContent','modalOpen'])
    const location = useLocation();
    const [opacity,setOpacity] = useState(1);
    const [scale,setScale] = useState(0)
    const [lastcontent,setLastContent] = useState(globalState.modalContent);
    const {pushToQueue} = useCallbackQueue(300);
    const [minimized,setMinimized] = useState(false);

    useEffect(() => {
        pushToQueue( () => {
            if(!globalModalRef.current)return;
            if(globalState.modalOpen){
            setOpacity(1)
            setScale(1)
            setLastContent(globalState.modalContent)
            globalModalRef.current.show()
            }else{
                setScale(0)
                setOpacity(0)
                setTimeout( () => {
                    setLastContent(null)
                },300)
            }
        })
    },[globalState.modalOpen,globalState.modalContent])

    //close the modal on route change
    useEffect( () => {
        return () => {
            dispatch({modalOpen:false,modalContent:null})
        }
    },[location])
    
    return (<>
        <dialog style={{opacity,transform:`scale(${scale}) translateY(-50%)`}} ref={globalModalRef} className='transition-all duration-[300ms] fixed shadow-lg top-[50%] w-[75%] max-w-[600px] z-[10] max-h-[75vh]'>
            <img className='absolute bottom-full top-[-70px] rotate-[-15deg] w-[100px]' src='/assets/totoro.png'/>
            <div className='absolute z-[1] top-[-14px] right-[20px] flex items-center gap-[5px]'>
                <button onClick={() => {
                    setOpacity(0)
                    setScale(0)
                    setMinimized(true);
                    setTimeout( () => {
                        dispatch({modalOpen:false})
                    },300)}
                } className='border-2 border-black rounded-[50%] bg-white transition-[0.2s] hover:rotate-[30deg] lg:hidden'>
                    <Minus/>
                </button>
                <button onClick={() => {
                    setOpacity(0)
                    setScale(0)
                    setTimeout( () => {
                        dispatch({modalOpen:false,modalContent:null})
                    },300)}
                } className='border-2 border-black rounded-[50%] bg-white hover:rotate-[90deg] transition-[0.2s]'>
                    <svg xmlns="http://www.w3.org/2000/svg" height="24" viewBox="0 -960 960 960" width="24"><path d="m256-200-56-56 224-224-224-224 56-56 224 224 224-224 56 56-224 224 224 224-56 56-224-224-224 224Z"/></svg>
                </button>
            </div>
            <div className='flex flex-col gap-[20px] p-[20px] relative bg-totorolightgreen border-2 border-black max-h-[75vh] overflow-y-scroll md:overflow-y-visible'>
                {lastcontent}
            </div>
        </dialog>
            <button 
                onClick={() => {
                    setMinimized(false);
                    dispatch({modalOpen:true})
                }}
                style={{opacity: minimized ? 1 - opacity : 0, pointerEvents: minimized ? 'all' : 'none'}} className='fixed bg-white bottom-0 z-[1] left-[50%] translate-x-[-50%] flex gap-[5px] items-center p-[5px] rounded shadow-lg transition-all duration-[300ms]'
            >
                <Triangle fill="black" width={10}/>
                <p>Reopen</p>
            </button>
        
    </>)
}