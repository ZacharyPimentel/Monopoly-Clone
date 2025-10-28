import { BoardSpace } from "@generated"

export const TaxTile:React.FC<{space:BoardSpace}> = ({space}) => {
    return (
        <div className='bg-[teal] flex items-center justify-center w-full h-full text-center shadow-lg border border-totorodarkgreen rounded-[5px]'>
            <p className='text-[6px] md:text-[12px] leading-tight md:leading-normal'>{space.boardSpaceName}</p>
        </div>
    )
}