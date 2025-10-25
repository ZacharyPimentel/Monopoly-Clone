import { useFormContext } from "react-hook-form"


export const NumberInput:React.FC<{
    formControl:string
    min?:number
    max?:number
    disabled?:boolean
}> = ({formControl,min,max,disabled = false}) => {

    const form = useFormContext();
    
    return (
        <input 
            className='pl-[5px] disabled:cursor-not-allowed'
            {...form.register(formControl,
                {
                    min,
                    max,
                    valueAsNumber:true,
                    setValueAs: (v) => v === "" ? 0 : Number(v)
                }
            )} 
            type='number'
            disabled={disabled}
            onFocus={(e) => {
                //make it a little more user friendly to users
                if(e.target.value === "0"){
                    e.target.value = ""
                }
            }}
            onBlur={(e) => {
                if(min !== undefined && parseInt(e.target.value) < min){
                    form.setValue(formControl, 0);
                }
                if(max !== undefined && parseInt(e.target.value) > max){
                    form.setValue(formControl,max)
                }
                if(e.target.value === ''){
                    form.setValue(formControl, 0);
                }
            }}
        />
    )
}