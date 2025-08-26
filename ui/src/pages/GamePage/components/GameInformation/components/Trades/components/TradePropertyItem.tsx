import { useFormContext } from "react-hook-form";
import { BoardSpace } from "@generated/index";
import { useMemo } from "react";

export const TradePropertyItem:React.FC<{space:BoardSpace, formControl:string}> = ({space,formControl}) => {
    const form = useFormContext();

    const gamePropertyId = space.property?.gamePropertyId
    const selectedPropertyIds = form.watch(formControl)

    const isPropertySelected = useMemo( () => {
        return selectedPropertyIds.includes(gamePropertyId)
    },[selectedPropertyIds,gamePropertyId])
    return (
        <button 
            onClick={() => {
            //remove if already selected
            if(isPropertySelected){
                form.setValue(
                    formControl,
                    selectedPropertyIds.filter( (id:number) => id !== gamePropertyId))
            }
            //add if not selected
            else{
                form.setValue(formControl,[...form.getValues(formControl),gamePropertyId])
            }
            
        }} className={`${isPropertySelected ? 'bg-black text-white' : "bg-white hover:bg-black hover:text-white"} flex justify-between w-full p-[5px]`}>
            {space.boardSpaceName} <span className='flex w-[20px] h-[20px] rounded-[20px]' style={{backgroundColor:space?.property?.color}}></span>
        </button>
    )
}