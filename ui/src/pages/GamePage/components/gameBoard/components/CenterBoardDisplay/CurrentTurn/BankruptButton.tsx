import { DeclareBankruptcyModal } from "@globalComponents/GlobalModal/modalContent/DeclareBankruptcyModal";
import { useGlobalContext } from "@stateProviders/GlobalStateProvider"

export const BankruptButton = () => {

    const {globalDispatch} = useGlobalContext();

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