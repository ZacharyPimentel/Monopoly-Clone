import { Player } from "@generated";
import { CompletePaymentModal } from "@globalComponents/GlobalModal/modalContent";
import { useGlobalState } from "@stateProviders";

export const CompletePaymentButton:React.FC<{player:Player}> = ({player}) => {

    const {dispatch:globalDispatch} = useGlobalState([]);

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