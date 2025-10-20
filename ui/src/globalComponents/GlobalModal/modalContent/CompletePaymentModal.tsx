import { Player } from "@generated";
import React from "react";
import { ActionButtons } from "@globalComponents/GlobalModal";
import { useWebSocket } from "@hooks";

export const CompletePaymentModal:React.FC<{player:Player}> = ({player}) => {

    const {invoke} = useWebSocket();

    return (
        <div className='flex flex-col gap-[20px]'>
            <p className='font-bold'>Pay Debt</p>
            {player.debts.length > 1 && (
                <p>You have multiple debts to settle. The first payment is below.</p>
            )}
            <p>Do you want to pay the required <b>${player.debts[0].amount}</b> payment?</p>
            <ActionButtons 
                confirmCallback={async() => {
                    invoke.player.completePayment();
                }} 
                confirmButtonStyle={"success"} 
                confirmButtonText={"Pay"}
                confirmDisabled={player.money < player.debts[0].amount}
            />
        </div>
    )
}