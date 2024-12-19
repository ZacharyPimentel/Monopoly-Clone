import { useEffect, useState } from "react"

export const OptionSelectMenu:React.FC<{
    apiCall?: () => Promise<any[]>,
    options?: any[]
    setStateCallback:(newValue:string) => void
    displayKey:string,
    defaultValue?:string
    additionalOptions?:{
        display:string
        value:string
    }[]
}> = ({apiCall,options=[],setStateCallback,displayKey,defaultValue,additionalOptions}) => {

    const [selectOptions,setSelectOptions] = useState<any[]>(options)
    const [loading,setLoading] = useState(apiCall ? true : false);
    const [value,setValue] = useState(defaultValue || '');

    useEffect( () => {
        if(!apiCall)return;
        (async () => {
            const result = await apiCall()
            if(!result)return
            setSelectOptions(result)
            setLoading(false)
        })()
    },[])

    if(loading){
        return (
            <select className='p-[5px] text-input'>
                <option>Loading...</option>
            </select>
        )
    }

    return (
        <select 
            onChange={(e) => {
                setValue(e.target.value)
                setStateCallback(e.target.value)} 
            }
            className='p-[5px] text-input'
            value={value}
        >
            <option value=''>Select One</option>
            {selectOptions.map( (option) => {
                return (
                    <option key={option.id} value={option.id}>{option[displayKey]}</option>
                )
            })}
            {additionalOptions?.map( (option) => {
                return (
                    <option key={option.value} value={option.value}>{option.display}</option>
                )
            })}
        </select>
    )
}