import { useGlobalDispatch } from "../../stateProviders/GlobalStateProvider";
import { useState } from "react";

export type AdvancedButtonConfig = {
    buttonStyle:'success' | 'warning'
    buttonCallback:Function
    buttonText:string
}

export const AdvancedActionButtons:React.FC<{buttonConfigs:AdvancedButtonConfig[]}> = ({buttonConfigs}) => {
    
    const globalDispatch = useGlobalDispatch();
    const [loading,setLoading] = useState(false);
    const [success,setSuccess] = useState(false);
    const [error,setError] = useState(false);
    
    return (
        <div className='flex gap-[20px] flex-wrap'>
            <button onClick={() => {
                globalDispatch({modalOpen:false,modalContent:null})
            }} className='min-w-[100px] bg-black text-white rounded p-[10px] hover:opacity-[0.8] transition-[0.2s] mr-auto'>
                Cancel
            </button>
            {buttonConfigs.map( (config,index) => {
                return (
                    <button key={index} onClick={() => runCallback(config.buttonCallback)} className={`${config.buttonStyle === 'success' && !error ? 'bg-lime-500':'bg-[tomato]'} min-w-[100px] rounded p-[10px] text-white disabled:bg-[lightgrey] enabled:hover:opacity-[0.8] transition-[0.2s] flex justify-center`}>
                        {config.buttonText}
                    </button>
                )
            })}
        </div>
    )

    async function runCallback(configCallback:Function){
        setLoading(true);
        
        configCallback()
            .then( () => {
                setLoading(false);
                setSuccess(true);
                setError(false);
                globalDispatch({modalOpen:false,modalContent:null})
            }).catch( (error: any) => {
                console.log(error)
                setLoading(false);
                setSuccess(false);
                setError(true)
            });
    }
}