import { BoardSpace, Property } from "@generated/index";
import React from "react";
import { useWebSocket } from "@hooks/useWebSocket";
import { ActionButtons } from "../ActionButtons";

export const UpgradePropertyModal:React.FC<{space:BoardSpace}> = ({space}) => {

    const {invoke} = useWebSocket();

    const property = space.property as Property

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Upgrade Property</p>
            <p>Do you want to upgrade <b>{space.boardSpaceName}</b> for <b>${property.upgradeCost}</b>?</p>
            <ActionButtons 
                confirmCallback={async() => {
                    if(!property.gamePropertyId)return
                    invoke.property.upgrade(property.gamePropertyId)
                }} 
                confirmButtonStyle={"success"} 
                confirmButtonText={"Upgrade"}
            />
        </div>
    )
}