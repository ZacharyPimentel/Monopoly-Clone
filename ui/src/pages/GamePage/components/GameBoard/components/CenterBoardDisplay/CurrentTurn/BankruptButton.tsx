import { DeclareBankruptcyModal } from "@globalComponents";
import { useGlobalState } from "@stateProviders";

export const BankruptButton = () => {

    const {dispatch:globalDispatch} = useGlobalState([]);

    return (
        <button
            onClick={() => {
                globalDispatch({modalOpen:true,modalContent:<DeclareBankruptcyModal/>})
            }}
            className='bg-white p-[5px] disabled:opacity-[0.6] min-w-[100px]'
        >
            Declare Bankruptcy
        </button>
    )
}