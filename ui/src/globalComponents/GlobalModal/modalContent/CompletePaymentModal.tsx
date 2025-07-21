import { Player } from "@generated/index";
import React from "react";
import { ActionButtons } from "../ActionButtons";
import { useWebSocket } from "@hooks/useWebSocket";

export const CompletePaymentModal:React.FC<{player:Player}> = ({player}) => {

    const {invoke} = useWebSocket();

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Pay Debt</p>
            <p>Do you want to pay the required <b>${player.moneyNeededForPayment}</b> payment?</p>
            <ActionButtons 
                confirmCallback={async() => {
                    invoke.player.completePayment();
                }} 
                confirmButtonStyle={"success"} 
                confirmButtonText={"Pay"}
                confirmDisabled={player.money < player.moneyNeededForPayment}
            />
        </div>
    )
}