import { BoardSpace, Property } from "@generated/index"
import { DowngradePropertyModal } from "@globalComponents/GlobalModal/modalContent/DowngradePropertyModal";
import { MortgagePropertyModal } from "@globalComponents/GlobalModal/modalContent/MortgagePropertyModal";
import { UnmortgagePropertyModal } from "@globalComponents/GlobalModal/modalContent/UnmortgagePropertyModal";
import { UpgradePropertyModal } from "@globalComponents/GlobalModal/modalContent/UpgradePropertyModal";
import { usePlayer } from "@hooks/usePlayer";
import { useGameState } from "@stateProviders/GameStateProvider"
import { useGlobalDispatch } from "@stateProviders/GlobalStateProvider";
import { useMemo } from "react";

export const Popover:React.FC<{
    property:Property,
    sideClass:string,
    space:BoardSpace
    propertyStyles:{position:string}
}> = ({property,space,propertyStyles}) => {

    const gameState = useGameState();
    const {player,isCurrentTurn} = usePlayer();
    const globalDispatch = useGlobalDispatch();
    
    const playerOwnsAllSetProperties = useMemo( () => {
        if(!property.setNumber) return false;
        const setSpaces = gameState.boardSpaces.filter((space) => space.property?.setNumber === property.setNumber);
        if(setSpaces.every(space => space?.property?.playerId === player?.id)){
            return true
        }
        return false
    },[property,isCurrentTurn])

    return (
        <div className={`${propertyStyles?.position} bg-white hidden group-hover:flex flex-col w-[150px] p-[5px] shadow-lg border border-black text-[12px]`}>
            <p className='mb-[5px]'>{space.boardSpaceName}</p>
            <div className='flex justify-between border-b border-black'>
                <p>Upgrades</p>
                <p>Rent</p>
            </div>
            {property.propertyRents.map( (propertyRent,index) => {
                return (
                    <div 
                        key={propertyRent.id}
                        style={{fontWeight:property.upgradeCount === index ? 'bold' : 'normal'}} 
                        className='flex justify-between'
                    >
                        <p>{propertyRent.upgradeNumber}</p>
                        <p>${propertyRent.rent}</p>
                    </div>
                )
            })}
            <div className='border-t border-black pt-[5px]'>
                <p>House Cost: ${property.upgradeCost}</p>
                <p>Mortgage Value: ${property.mortgageValue}</p>
            </div>
            {playerOwnsAllSetProperties && isCurrentTurn && property.upgradeCount < 5 && (
                <div className='mt-[10px]'>
                    <button onClick={() => {
                        globalDispatch({modalOpen:true,modalContent:<UpgradePropertyModal space={space}/>})
                    }} className={'bg-totorolightgreen p-[5px] w-full'}>
                        Upgrade
                    </button>
                </div>
            )}
            {playerOwnsAllSetProperties && isCurrentTurn && property.upgradeCount !== 0 && (
                <div className='mt-[10px]'>
                    <button onClick={() => {
                        globalDispatch({modalOpen:true,modalContent:<DowngradePropertyModal space={space}/>})
                    }} className={'bg-totorolightgreen p-[5px] w-full'}>
                        Downgrade
                    </button>
                </div>
            )}
            {property.playerId === player?.id && isCurrentTurn && property.upgradeCount === 0 && (
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