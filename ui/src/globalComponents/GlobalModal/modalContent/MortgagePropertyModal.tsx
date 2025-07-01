import { BoardSpace, Property } from "@generated/index";
import React from "react";
import { useWebSocket } from "@hooks/useWebSocket";
import { ActionButtons } from "../ActionButtons";

export const MortgagePropertyModal:React.FC<{space:BoardSpace}> = ({space}) => {

    const {invoke} = useWebSocket();

    const property = space.property as Property

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Mortgage Property</p>
            <p>Do you want to mortgage <b>{space.boardSpaceName}</b> for <b>${property.mortgageValue}</b>?</p>
            <ActionButtons 
                confirmCallback={async() => {
                    if(!property.gamePropertyId)return
                    invoke.property.mortgage(property.gamePropertyId)
                }} 
                confirmButtonStyle={"warning"} 
                confirmButtonText={"Mortgage"}
            />
        </div>
    )
}