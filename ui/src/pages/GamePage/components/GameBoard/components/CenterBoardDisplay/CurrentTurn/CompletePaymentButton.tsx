import { Player } from "@generated";
import { CompletePaymentModal } from "@globalComponents";
import { useGlobalState } from "@stateProviders";

export const CompletePaymentButton:React.FC<{player:Player}> = ({player}) => {

    const {dispatch:globalDispatch} = useGlobalState([]);

    return (
        <button
            onClick={() => {
                globalDispatch({modalContent:<CompletePaymentModal player={player}/>,modalOpen:true})
            }}
            className='game-button'
        >
            Complete Payment
        </button>
    )
}