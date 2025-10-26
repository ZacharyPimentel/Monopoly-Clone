import { X } from "lucide-react"
import { MultiSelectData } from "../MultiSelect"
import { fixColorContrast } from "src/helpers/fixColorContrast"
import { useFormContext } from "react-hook-form"

export const MultiSelectSelection:React.FC<{selection:MultiSelectData, formControlPrefix:string}> = ({selection, formControlPrefix}) => {

    const form = useFormContext();

    return (
        <div className='border flex gap-[5px] px-[5px] w-fit relative'>
            <p style={{color:fixColorContrast(selection.color,selection.color)}} title={selection.display} className='truncate max-w-[50px] relative z-[1]'>{selection.display}</p>
            <span className='absolute w-full h-full top-0 left-0' style={{opacity:0.5,backgroundColor:selection.color}}></span>
            <button className='relative' onClick={(e) => {
                e.stopPropagation();
                const formControlValues = form.getValues(formControlPrefix);
                const newValues = formControlValues.filter((value:string|number) => value !== selection.value)
                form.setValue(formControlPrefix, newValues)
            }}>
                <X size={14} className='hover:bg-black hover:stroke-white'/>
            </button>
        </div>
    )
}