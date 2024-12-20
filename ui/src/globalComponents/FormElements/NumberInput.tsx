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
            className='pl-[5px]'
            {...form.register(formControl,{min,max})} 
            type='number'
            disabled={disabled}
            onBlur={(e) => {
                if(min !== undefined && parseInt(e.target.value) < min){
                    form.setValue(formControl, 0);
                }
                console.log(max)
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