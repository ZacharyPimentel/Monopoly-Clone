import { DieSide } from "./DieSide"

export const Die:React.FC<{value:number}> = ({value}) => {

    return (
        <div style={{transformStyle:'preserve-3d'}} className={`show-${value} die relative w-[100px] h-[100px]`}>
            {Array.from({length:6}).map( (_,index) => {
                return (
                    <DieSide key={index} currentSide={value === index+1} side={index + 1}/>
                )
            })}
        </div>
    )
}