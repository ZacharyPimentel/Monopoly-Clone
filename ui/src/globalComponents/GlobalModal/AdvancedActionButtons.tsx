import { useGlobalState } from "@stateProviders";
import { useState } from "react";

export type AdvancedButtonConfig = {
    buttonStyle:'success' | 'warning'
    buttonCallback:Function
    buttonText:string
    closeOnAction?:boolean
    disabled?:boolean
}

export const AdvancedActionButtons:React.FC<{buttonConfigs:AdvancedButtonConfig[]}> = ({buttonConfigs}) => {
    
    const {dispatch:globalDispatch} = useGlobalState([]);
    const [_loading,setLoading] = useState(false);
    const [_success,setSuccess] = useState(false);
    const [error,setError] = useState(false);
    
    return (
        <div className='flex gap-[20px] flex-wrap'>
            <button onClick={() => {
                globalDispatch({modalOpen:false,modalContent:null})
            }} className='min-w-[100px] bg-black text-white rounded p-[10px] hover:opacity-[0.8] transition-[0.2s] mr-auto'>
                Close
            </button>
            {buttonConfigs.map( (config,index) => {
                return (
                    <button disabled={config.disabled ?? false} key={index} onClick={() => runCallback(config.buttonCallback, config.closeOnAction)} className={`${config.buttonStyle === 'success' && !error ? 'bg-lime-500':'bg-[tomato]'} min-w-[100px] rounded p-[10px] text-white disabled:bg-[lightgrey] enabled:hover:opacity-[0.8] transition-[0.2s] flex justify-center`}>
                        {config.buttonText}
                    </button>
                )
            })}
        </div>
    )

    async function runCallback(configCallback:Function, closeOnAction:boolean = true){
        setLoading(true);
        
        configCallback()
            .then( () => {
                setLoading(false);
                setSuccess(true);
                setError(false);
                if(closeOnAction){
                    globalDispatch({modalOpen:false,modalContent:null})
                }
            }).catch( (error: any) => {
                console.log(error)
                setLoading(false);
                setSuccess(false);
                setError(true)
            });
    }
}