import { BoardSpace, Property } from "@generated/index"
import { MortgagePropertyModal } from "@globalComponents/GlobalModal/modalContent/MortgagePropertyModal";
import { UnmortgagePropertyModal } from "@globalComponents/GlobalModal/modalContent/UnmortgagePropertyModal";
import { usePlayer } from "@hooks/usePlayer";
import { useGlobalDispatch } from "@stateProviders/GlobalStateProvider";

export const UtilityPopover:React.FC<{
    property:Property,
    sideClass:string,
    space:BoardSpace
    propertyStyles:{position:string}
}> = ({property,space,propertyStyles}) => {

    const {player,isCurrentTurn} = usePlayer();
    const globalDispatch = useGlobalDispatch();

    return (
        <div className={`${propertyStyles?.position} bg-white hidden group-hover:flex flex-col w-[150px] p-[5px] shadow-lg border border-black text-[12px]`}>
            <p className='mb-[5px]'>{space.boardSpaceName}</p>
            <div className='flex justify-between border-b border-black'>
                <p>Owned</p>
                <p>Payment</p>
            </div>
            <div className='flex justify-between'>
                <p>1</p>
                <p>4x Dice Roll</p>
            </div>
            <div className='flex justify-between'>
                <p>2</p>
                <p>10x Dice Roll</p>
            </div>
            <div className='border-t border-black pt-[5px]'>
                <p>Mortgage Value: ${property.mortgageValue}</p>
            </div>
            {property.playerId === player?.id && isCurrentTurn && (
                <div className='mt-[10px]'>
                    <button onClick={() => {
                        if(!property.mortgaged){
                            globalDispatch({
                                modalOpen:true,
                                modalContent: <MortgagePropertyModal space={space}/> 
                            })
                        }else{
                            globalDispatch({
                                modalOpen:true,
                                modalContent: <UnmortgagePropertyModal space={space}/> 
                            })
                        }
                    }} className={`${property.mortgaged ? 'bg-totorolightgreen' : 'bg-[tomato]'} p-[5px] w-full`}>
                        {space?.property?.mortgaged
                            ? 'Unmortgage'
                            : 'Mortgage'
                        }
                    </button>
                </div>
            )}
        </div>
    )
}