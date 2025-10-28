import { fixColorContrast } from "src/helpers/fixColorContrast"
import { MultiSelectData } from "../MultiSelect"
import { useFormContext } from "react-hook-form"

export const MultiselectOption:React.FC<{option:MultiSelectData, formControlPrefix:string}> = ({option, formControlPrefix}) => {

    const form = useFormContext();

    return (
        <button className='p-[5px] relative group w-full text-left flex items-center gap-[5px]' onClick={() => {
            const formValues = form.getValues(formControlPrefix)
            const newValues = [...formValues,option.value].sort();
            form.setValue(formControlPrefix,newValues)
        }}>
            <span style={{backgroundColor:option.color}} className='flex min-w-[15px] h-[15px] rounded-full'></span>
            <p title={option.display} className='relative z-[1] gap-[5px] truncate' style={{color:fixColorContrast(option.color,option.color)}}>
                {option.display}
            </p>
            <span style={{opacity:0.5,backgroundColor:option.color}} className='absolute inset-0 hidden group-hover:flex'></span>
        </button>
    )
}