import { Player } from "@generated/index";
import { CompletePaymentModal } from "@globalComponents/GlobalModal/modalContent/CompletePaymentModal";
import { useGlobalDispatch } from "@stateProviders/GlobalStateProvider"

export const CompletePaymentButton:React.FC<{player:Player}> = ({player}) => {

    const globalDispatch = useGlobalDispatch();

    return (
        <button
            onClick={() => {
                globalDispatch({modalContent:<CompletePaymentModal player={player}/>,modalOpen:true})
            }}
            className='bg-white p-[5px] disabled:opacity-[0.6] min-w-[100px]'
        >
            Complete Payment
        </button>
    )
}