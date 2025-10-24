import { BoardSpace, Property } from "@generated";
import React from "react";
import { useWebSocket } from "@hooks";
import { ActionButtons } from "@globalComponents";

export const UnmortgagePropertyModal:React.FC<{space:BoardSpace}> = ({space}) => {

    const {invoke} = useWebSocket();

    const property = space.property as Property

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Unmortgage Property</p>
            <p>Do you want to unmortgage <b>{space.boardSpaceName}</b> for <b>${Math.round(property.mortgageValue * 1.1)}</b>?</p>
            <ActionButtons 
                confirmCallback={async() => {
                    if(!property.gamePropertyId)return
                    invoke.property.unmortgage(property.gamePropertyId)
                }} 
                confirmButtonStyle={"success"} 
                confirmButtonText={"Unmortgage"}
            />
        </div>
    )
}