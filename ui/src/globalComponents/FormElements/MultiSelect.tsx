import { ChevronDown, X } from "lucide-react"
import { SubmenuWrapper } from "../SubmenuWrapper"
import { Fragment, useEffect, useMemo, useRef, useState } from "react";
import { MultiselectOption } from "./components/MultiSelectOption";
import { MultiSelectSelection } from "./components/MultiSelectSelection";
import { useFormContext } from "react-hook-form";

export type MultiSelectData = {
    id: string | number
    display:string
    value:string | number
    color:string
}

export const MultiSelect:React.FC<{data:MultiSelectData[], formControlPrefix:string}> = ({data, formControlPrefix}) => {
    
    const form = useFormContext();

    const selected:(string|number)[] = form.watch(`${formControlPrefix}`)
    
    const [submenuOpen,setSubmenuOpen] = useState(false);
    const multiSelectRef = useRef<HTMLDivElement>(null)

    const selectedData = useMemo( () => {
        return data.filter( (item) => selected.includes(item.id))
    },[selected])

    const unselectedData = useMemo( () => {
        return data.filter( (item) => !selected.includes(item.id))
    },[selected])

    useEffect( () => {
        const multiSelectListener = (event:any) => {
            if(!multiSelectRef.current) return
            if(multiSelectRef.current.contains(event.target)){
                setSubmenuOpen(!submenuOpen)
            }else{
                setSubmenuOpen(false)
            }
        }

        window.addEventListener("click",multiSelectListener)

        return () => {
            window.removeEventListener("click",multiSelectListener)
        }

    },[submenuOpen])


    return (
        <div ref={multiSelectRef} className='flex flex-col relative'>
            
            <div className='bg-white flex py-[5px] pl-[5px] cursor-pointer'>
                <div className='flex flex-wrap gap-[10px] text-[14px] items-center'>
                    {selectedData.length === 0 && (
                        <p className='opacity-[0.5]'>Select Properties...</p>
                    )}
                    {selectedData.map( (item) => {
                        return (
                            <Fragment key={item.id}>
                                <MultiSelectSelection formControlPrefix={formControlPrefix} selection={item}/>
                            </Fragment>
                        )
                    })}
                </div>
                <div className='ml-auto px-[5px] flex'>
                    <button onClick={(e) => {
                        e.stopPropagation(); 
                        form.setValue(formControlPrefix,[]);
                    }} className='border-r-2 flex h-fit hover:bg-black group'
                    >
                        <X className='group-hover:stroke-white' size={24}/>
                    </button>
                    <button className='flex h-fit'>
                        <ChevronDown size={24}/>
                    </button>
                </div>
            </div>
            {/* Clicks on the select options prevented from closing the dropdown */}
            <div onClick={(e) => e.stopPropagation()} className='mt-[5px]'>
                <SubmenuWrapper side={'left'} submenuOpen={submenuOpen}>
                    {unselectedData.length === 0 && (
                        <p className='italic opacity-[0.7] p-[5px]'>No options available</p>
                    )}
                    {unselectedData.map( (item) => {
                        return (
                            <Fragment key={item.id}>
                                <MultiselectOption formControlPrefix={formControlPrefix} option={item} />
                            </Fragment>
                        )
                    })}
                </SubmenuWrapper>
            </div>
        </div>
        
    )
}