import { BoardSpace } from "@generated/index"

export const TaxTile:React.FC<{space:BoardSpace}> = ({space}) => {
    return (
        <div className='bg-[teal] flex items-center justify-center w-full h-full text-center shadow-lg border border-totorodarkgreen rounded-[5px]'>
            <p className='text-[12px]'>{space.boardSpaceName}</p>
        </div>
    )
}