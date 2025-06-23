import { useFormContext } from "react-hook-form";
import { BoardSpace } from "@generated/index";

export const TradePropertyItem:React.FC<{space:BoardSpace, formControl:string}> = ({space,formControl}) => {
    const form = useFormContext();

    const gamePropertyId = space.property?.gamePropertyId
    const selectedPropertyIds = form.watch(formControl)
    return (
        <button 
            style={{backgroundColor: selectedPropertyIds.includes(gamePropertyId) ? 'black' : "white"}}
            onClick={() => {
            //remove if already selected
            if(selectedPropertyIds.includes(gamePropertyId)){
                form.setValue(
                    formControl,
                    selectedPropertyIds.filter( (id:number) => id !== gamePropertyId))
            }
            //add if not selected
            else{
                form.setValue(formControl,[...form.getValues(formControl),gamePropertyId])
            }
            
        }} className='flex w-full bg-white p-[5px] hover:bg-black hover:text-white'>
            {space.boardSpaceName}
        </button>
    )
}