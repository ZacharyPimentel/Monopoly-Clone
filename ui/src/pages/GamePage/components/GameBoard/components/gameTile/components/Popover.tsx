import { BoardSpace, Property } from "@generated"
import { 
    DowngradePropertyModal,
    MortgagePropertyModal,
    UnmortgagePropertyModal,
    UpgradePropertyModal
} from "@globalComponents";
import { usePlayer } from "@hooks";
import { useGameState, useGlobalState } from "@stateProviders";

export const Popover:React.FC<{
    property:Property,
    sideClass:string,
    space:BoardSpace
    propertyStyles:{position:string}
}> = ({property,space,propertyStyles}) => {

    const gameState = useGameState(['boardSpaces','game']);
    const {player,isCurrentTurn} = usePlayer();
    const {dispatch:globalDispatch} = useGlobalState([]);

    const setSpaces = gameState.boardSpaces.filter((space) => space.property?.setNumber === property.setNumber);
    const playerOwnsAllSetProperties = setSpaces.every(space => space?.property?.playerId === player?.id);
    const anySetPropertyIsMortgaged = !setSpaces.every(space => !space?.property?.mortgaged)
    const anySetPropertyIsUpgraded = !setSpaces.every(space => !space?.property?.upgradeCount || 0 > 0)
    const allowedToBuildUnevenlySettingOn = gameState.game?.allowedToBuildUnevenly

    const minUpgradeCount = Math.min(...setSpaces.map(s => s?.property?.upgradeCount || 0))
    const maxUpgradeCount = Math.max(...setSpaces.map(s => s?.property?.upgradeCount || 0))

    const sharedImprovementConditions =
        playerOwnsAllSetProperties &&
        isCurrentTurn &&
        !property.mortgaged &&
        !anySetPropertyIsMortgaged;

    const upgradeAllowed =
        sharedImprovementConditions &&
        property.upgradeCount < 5 &&
        (allowedToBuildUnevenlySettingOn ||property.upgradeCount + 1 <= minUpgradeCount + 1);

    const downgradeAllowed =
        sharedImprovementConditions &&
        property.upgradeCount > 0 &&
        (allowedToBuildUnevenlySettingOn ||property.upgradeCount - 1 >= maxUpgradeCount - 1);

    const mortgageInteractionAllowed =
        property.playerId === player?.id &&
        isCurrentTurn &&
        property.upgradeCount === 0 &&
        !anySetPropertyIsUpgraded

    const unmortgageAllowed = 
        !property.mortgaged || (property.mortgaged && player.money > property.mortgageValue * 1.1)

    return (
        <div className={`${propertyStyles?.position} bg-white hidden group-hover:flex flex-col w-[100px] md:w-[200px] p-[5px] shadow-lg border border-black text-[12px]`}>
            <p className='game-text mb-[5px] font-bold'>{space.boardSpaceName}</p>
            <div className='flex justify-between border-b border-black'>
                <p className='game-text'>Upgrades</p>
                <p className='game-text'>Rent</p>
            </div>
            {property.propertyRents.map( (propertyRent,index) => {
                return (
                    <div 
                        key={propertyRent.id}
                        style={{fontWeight:property.upgradeCount === index ? 'bold' : 'normal'}} 
                        className='flex justify-between'
                    >
                        <p className='game-text'>{propertyRent.upgradeNumber}</p>
                        <p className='game-text'>${propertyRent.rent}</p>
                    </div>
                )
            })}
            <div className='border-t border-black pt-[5px]'>
                <p className='game-text'>House Cost: <b>${property.upgradeCost}</b></p>
                <p className='game-text'>Mortgage Value: <b>${property.mortgageValue}</b></p>
            </div>
            {upgradeAllowed &&(
                <div className='mt-[10px] flex gap-[5px]'>
                    <button onClick={() => {
                        globalDispatch({modalOpen:true,modalContent:<UpgradePropertyModal space={space}/>})
                    }} className={'bg-totorolightgreen p-[5px] w-full text-[8px] md:text-[12px] rounded'}>
                        Upgrade
                    </button>
                    {/* <button title={'Bulk Upgrade'} className={'bg-totorolightgreen p-[5px] text-[8px] md:text-[12px] rounded'}>
                        <Layers size={20}/>
                    </button> */}
                </div>
            )}
            {downgradeAllowed && (
                <div className='mt-[10px]'>
                    <button onClick={() => {
                        globalDispatch({modalOpen:true,modalContent:<DowngradePropertyModal space={space}/>})
                    }} className={'bg-totorolightgreen p-[5px] w-full text-[8px] md:text-[12px] rounded'}>
                        Downgrade
                    </button>
                </div>
            )}
            {mortgageInteractionAllowed && unmortgageAllowed && (
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
                    }} className={`${property.mortgaged ? 'bg-totorolightgreen' : 'bg-[tomato]'} p-[5px] w-full text-[8px] md:text-[12px] rounded`}>
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