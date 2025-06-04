import { useEffect, useState } from "react"
import { DieSide } from "./DieSide"

export const Die:React.FC<{value:number}> = ({value}) => {

    const [lastValue,setLastValue] = useState(value)
    const [rollSimulationValue,setRollSimulationValue] = useState(null);

    useEffect( () => {
        if(value === lastValue){
            setLastValue(value)
        }
    },[value,rollSimulationValue])

    return (
        <div style={{transformStyle:'preserve-3d',transition:'transform 1s'}} className={`show-${value} die relative w-[100px] h-[100px]`}>
            
            {Array.from({length:6}).map( (_,index) => {
                return (
                    <DieSide key={index} currentSide={value === index+1} side={index + 1}/>
                )
            })}
        </div>
    )
}