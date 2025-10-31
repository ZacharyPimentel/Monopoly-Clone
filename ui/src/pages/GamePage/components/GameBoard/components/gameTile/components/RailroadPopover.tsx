import { BoardSpace, Property } from "@generated"
import { MortgagePropertyModal, UnmortgagePropertyModal } from "@globalComponents";
import { usePlayer } from "@hooks";
import { useGlobalState } from "@stateProviders";
import { useMemo } from "react";

export const RailroadPopover:React.FC<{
    property:Property,
    sideClass:string,
    space:BoardSpace
    propertyStyles:{position:string}
}> = ({property,space,propertyStyles}) => {

    const {player,isCurrentTurn} = usePlayer();
    const {dispatch:globalDispatch} = useGlobalState([]);

    const railroadRents = useMemo( () => {
        return [25,50,100,200]
    },[])

    return (
        <div className={`${propertyStyles?.position} bg-white hidden group-hover:flex flex-col w-[100px] md:w-[200px] p-[5px] shadow-lg border border-black text-[12px]`}>
            <p className='game-text mb-[5px] font-bold'>{space.boardSpaceName}</p>
            <div className='flex justify-between border-b border-black'>
                <p className='game-text'>Owned</p>
                <p className='game-text'>Rent</p>
            </div>
            {railroadRents.map( (rent,index) => {
                return (
                    <div 
                        key={rent}
                        className='flex justify-between'
                    >
                        <p className='game-text'>{index + 1}</p>
                        <p className='game-text'>${rent}</p>
                    </div>
                )
            })}
            <div className='border-t border-black pt-[5px]'>
                <p>Mortgage Value: <b>${property.mortgageValue}</b></p>
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
                    }} className={`${property.mortgaged ? 'bg-totorolightgreen' : 'bg-[tomato]'} p-[5px] w-full text-[8px] md:text-[12px]`}>
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