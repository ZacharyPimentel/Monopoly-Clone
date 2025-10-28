import { DeclareBankruptcyModal } from "@globalComponents";
import { useGlobalState } from "@stateProviders";

export const BankruptButton = () => {

    const {dispatch:globalDispatch} = useGlobalState([]);

    return (
        <button
            onClick={() => {
                globalDispatch({modalOpen:true,modalContent:<DeclareBankruptcyModal/>})
            }}
            className='game-button'
        >
            Declare Bankruptcy
        </button>
    )
}