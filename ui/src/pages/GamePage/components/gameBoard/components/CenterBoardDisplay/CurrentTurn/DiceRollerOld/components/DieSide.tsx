import { useEffect, useMemo } from "react"
import { SideDot } from "./SideDot"

export const DieSide:React.FC<{side:number,currentSide:boolean}> = ({side,currentSide}) => {

    const dotPositions = useMemo( () => {
        if(side === 1){
            return [
                {
                    top:50,
                    left:50
                }
            ]
        }
        if(side === 2){
            return [
                {
                   top:20,
                   left:20 
                },
                {
                    top:80,
                    left:80
                 },
            ]
        }
        if(side === 3){
            return [
                {
                   top:20,
                   left:20 
                },
                {
                    top:50,
                    left:50
                },
                {
                    top:80,
                    left:80
                 },

            ]
        }
        if(side === 4){
            return [
                {
                   top:20,
                   left:20 
                },
                {
                    top:20,
                    left:80
                },
                {
                    top:80,
                    left:80
                 },
                 {
                    top:80,
                    left:20 
                 },
            ]
        }
        if(side === 5){
            return [
                {
                   top:20,
                   left:20 
                },
                {
                    top:20,
                    left:80
                },
                {
                    top:80,
                    left:80
                 },
                 {
                    top:80,
                    left:20 
                 },
                 {
                    top:50,
                    left:50
                 }
            ]
        }
        if(side === 6){
            return [
                {
                   top:20,
                   left:20 
                },
                {
                    top:20,
                    left:80
                },
                {
                    top:50,
                    left:20
                 },
                 {
                    top:50,
                    left:80 
                 },
                 {
                    top:80,
                    left:20
                 },
                 {
                    top:80,
                    left:80
                 }
            ]
        }
    },[currentSide])

    if(!dotPositions) return null

    return (
        <div className={`side side-${side} absolute bg-white rounded-[5px] w-[100px] h-[100px] border border-[#e5e5e5] text-center`}>
            {dotPositions!.map( (position,index) => {
                return (
                    <SideDot key={index} top={position.top} left={position.left}/>
                )
            })}
        </div>
    )
}