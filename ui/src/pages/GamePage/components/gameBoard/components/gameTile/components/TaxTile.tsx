import { BoardSpace } from "../../../../../../../types/controllers/BoardSpace"

export const TaxTile:React.FC<{space:BoardSpace}> = ({space}) => {
    return (
        <div className='flex items-center justify-center w-full h-full text-center shadow-lg border border-totorodarkgreen'>
            <p className='text-[12px]'>{space.boardSpaceName}</p>
        </div>
    )
}