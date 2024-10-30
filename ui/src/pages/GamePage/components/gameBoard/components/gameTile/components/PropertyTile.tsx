import { useMemo } from "react";
import { useGameState } from "../../../../../../../global/GameStateProvider";
import { setTilePositions } from "../../../../../../../helpers/tilePositions";
import { getPropertyInformation } from "../../../../../../../helpers/getPropertyInformation";

export const PropertyTile:React.FC<{position:number}> = ({position}) => {

    const gameState = useGameState();

    const propertyInformation = useMemo( () => {
        return getPropertyInformation(gameState.theme,position);
    },[position])

    const propertySetNumber = useMemo( () => {
        return setTilePositions.findIndex( (set) => set.includes(position)) + 1;
    },[position])

    //return the same content but in a different orientation depending on the property set

    if(propertySetNumber === 1 || propertySetNumber === 2){
        return (
            <button className='w-full h-full bg-white flex flex-col items-center justify-between shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden'>
                <p className='text-center bg-[#eaeaea] px-[5px]'>${propertyInformation?.values?.purchasePrice}</p>
                <p className='absolute w-full top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%]'>{propertyInformation?.name}</p>
                <span className={`rounded-b-[4px] h-[20%] w-full flex ${propertySetNumber === 1 ? 'bg-monopolyBrown' : 'bg-monopolyLightBlue'}`}></span>
            </button>
        )
    }

    if(propertySetNumber === 3 || propertySetNumber === 4){
        return (
            <button className='w-full h-full bg-white flex items-center justify-between scale-[-1] shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden'>
                <p className='[writing-mode:vertical-lr] text-center bg-[#eaeaea] px-[5px]'>${propertyInformation?.values?.purchasePrice}</p>
                <p className='h-full [writing-mode:vertical-lr] absolute top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%]'>{propertyInformation?.name}</p>
                <span className={`rounded-r-[4px] w-[20%] h-full flex ${propertySetNumber === 3 ? 'bg-monopolyPink' : 'bg-monopolyOrange'}`}></span>
            </button>
        )
    }

    if(propertySetNumber === 5 || propertySetNumber === 6){
        return (
            <button className='w-full h-full bg-white flex flex-col-reverse items-center justify-between relative shadow-lg border border-totorodarkgreen rounded-[5px] overflow-hidden'>
                <p className='text-center bg-[#eaeaea] px-[5px]'>${propertyInformation?.values?.purchasePrice}</p>
                <p className='w-full absolute top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%]'>{propertyInformation?.name}</p>
                <span className={`rounded-t-[4px] h-[20%] w-full flex ${propertySetNumber === 5 ? 'bg-monopolyRed' : 'bg-monopolyYellow'}`}></span>
            </button>
        )
    }

    if(propertySetNumber === 7 || propertySetNumber === 8){
        return (
            <button className='w-full h-full bg-white flex items-center justify-between border border-totorodarkgreen rounded-[5px] shadow-lg overflow-hidden'>
                <p className='break-keep [writing-mode:vertical-rl] text-center bg-[#eaeaea] px-[5px]'>${propertyInformation?.values?.purchasePrice}</p>
                <p className='h-full break-keep [writing-mode:vertical-rl] absolute top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%]'>{propertyInformation?.name}</p>
                <span className={`rounded-r-[4px] w-[20%] h-full flex ${propertySetNumber === 7 ? 'bg-monopolyGreen' : 'bg-monopolyBlue'}`}></span>
            </button>
        )
    }
}