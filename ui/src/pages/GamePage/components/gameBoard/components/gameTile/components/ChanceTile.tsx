import { BoardSpace } from "../../../../../../../types/controllers/BoardSpace"

export const ChanceTile:React.FC<{space:BoardSpace}> = ({space}) => {
    return (
        <div className='bg-[green] flex items-center justify-center w-full h-full shadow-lg border border-totorodarkgreen rounded-[5px]'>
            <p className='text-[12px]'>{space.boardSpaceName}</p>
        </div>
    )
}