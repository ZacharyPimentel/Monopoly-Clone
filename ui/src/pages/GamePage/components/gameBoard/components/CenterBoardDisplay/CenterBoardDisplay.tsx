import { DiceRoller } from "../DiceRoller/DiceRoller"

export const CenterBoardDisplay = () => {
    return (
        <div className='bg-[rgba(255,255,255,1)] rounded p-[5px] absolute z-[1] top-[50%] left-[50%] translate-x-[-50%] translate-y-[-50%] w-[25%] h-[25%]'>
            <DiceRoller/>
        </div>
    )
}