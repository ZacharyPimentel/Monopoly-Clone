import { useGlobalState } from "@stateProviders";
import { useCallback, useMemo } from "react";

type ErrorResponse = {
    status:string
    errors: { [key: string]: string[] }
}

export const useHttp = () => {
    const {dispatch:globalDispatch} = useGlobalState([]);

    const formatErrors = useCallback( async(res:ErrorResponse) => {
        const messages = [res.status + ' Error']
        Object.keys(res.errors).forEach( (key) => {
            res.errors[key].forEach( (error) => {
                messages.push(error)
            })
        })
        return messages
    },[])

    const http = useMemo( () => {
        return {
            get: async(url:string,queryParams:{[key:string]:string | number | boolean} = {}) => {
                const urlObject = new URL(url);
                Object.keys(queryParams).forEach( (key) =>{
                    urlObject.searchParams.append(key,queryParams[key].toString())
                })
                const response = await fetch(urlObject.toString(),{
                    method:'GET',
                    headers:{
                        "Content-Type":"application/json"
                    }
                })
                if(response.status !== 200) return undefined
                const json = await response.json();
                return json;
            },
            post: async(url:string,params:any) => {

                const response = await fetch(url,{
                    method:'POST',
                    body:JSON.stringify(params),
                    headers:{
                        "Content-Type":"application/json"
                    }
                })
                const json = await response.json();
                if(response.status !== 200){
                    globalDispatch({
                        toastOpen:true,
                        toastStyle:'error',
                        toastMessages: await formatErrors(json)
                    })
                    return undefined
                }
                return json;
            },
            put: async(url:string,params:any) => {

                const response = await fetch(url,{
                    method:'PUT',
                    body:JSON.stringify(params),
                    headers:{
                        "Content-Type":"application/json"
                    }
                })
                if(response.status !== 200) return undefined
                const json = await response.json();
                return json;
            },
            patch: async(url:string,params:any) => {

                const response = await fetch(url,{
                    method:'PATCH',
                    body:JSON.stringify(params),
                    headers:{
                        "Content-Type":"application/json"
                    }
                })
                if(response.status !== 200) return undefined
                const json = await response.json();
                return json;
            },
            delete: async(url:string) => {
                const response = await fetch(url,{method:'DELETE'})
                if(response.status !== 204) return undefined
                return {deleted:true}
            }
        }
    },[]);

    return http
}