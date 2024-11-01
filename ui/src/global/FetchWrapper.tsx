import { ReactNode, useEffect, useState } from "react";

export const FetchWrapper:React.FC<{apiCall:Function,children:((data: any) => React.ReactNode)}> = ({apiCall,children}) => {
    
    const [response,setResponse] = useState<any>(null)
    const [loading,setLoading] = useState(true)

    

    useEffect( () => {
        (async () => {
            const response = await apiCall();
            setResponse(response)
            setLoading(false)
        })()
    },[])

    if(loading){
        return <p>Loading...</p>
    }

    return children(response)
}