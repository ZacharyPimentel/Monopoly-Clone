import { BoardSpace, Property } from "@generated/index";
import React from "react";
import { useWebSocket } from "@hooks/useWebSocket";
import { ActionButtons } from "../ActionButtons";

export const DowngradePropertyModal:React.FC<{space:BoardSpace}> = ({space}) => {

    const {invoke} = useWebSocket();

    const property = space.property as Property

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Downgrade Property</p>
            <p>Do you want to downgrade <b>{space.boardSpaceName}</b> for <b>${(property.upgradeCost/2).toFixed(2)}</b>?</p>
            <ActionButtons 
                confirmCallback={async() => {
                    if(!property.gamePropertyId)return
                    invoke.property.downgrade(property.gamePropertyId)
                }} 
                confirmButtonStyle={"warning"} 
                confirmButtonText={"Downgrade"}
            />
        </div>
    )
}