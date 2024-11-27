import { useEffect, useState } from "react"

export const CheckboxMultiSelect:React.FC<{
    apiCall?: () => Promise<any[]>,
    options?: {
        id:string,
        [key:string]:string
    }[]
    setStateCallback:(newValues:string[]) => void
    displayKey:string
}> = ({apiCall,setStateCallback,displayKey,options}) => {

    const [checkboxOptions,setCheckboxOptions] = useState<any[]>(options || [])
    const [loading,setLoading] = useState(true)
    const [selectedOptions,setSelectedOptions] = useState<string[]>([])

    //fetch the data
    useEffect( () => {
        if(!apiCall){
            setLoading(false)
            return
        }
        (async () => {
            const result = await apiCall()
            setCheckboxOptions(result)
            setLoading(false)
        })()
    },[])

    //update parent state when local state changes
    useEffect( () => {
        setStateCallback(selectedOptions)
    },[selectedOptions])

    if(loading){
        return <p>Loading...</p>
    }

    return (
        <ul className='flex flex-wrap gap-x-[20px] gap-y-[10px]'>
            {checkboxOptions.map( (option) => {
                return (
                    <li key={option.id}>
                        <label className='flex gap-[20px] items-center w-fit'>
                            <p>{option[displayKey]}</p>
                            <input checked={selectedOptions.includes(option.id)} onChange={(e) => {
                                const updatedState = [...selectedOptions]
                                if(e.target.checked){
                                    updatedState.push(option.id)
                                }else{
                                    setSelectedOptions(updatedState.filter((id) => id !== option.id))
                                    return
                                }
                                setSelectedOptions(updatedState)
                            }} type='checkbox' className='scale-[1.5]'/>
                        </label>
                    </li>
                )
            })}
        </ul>
    )
}