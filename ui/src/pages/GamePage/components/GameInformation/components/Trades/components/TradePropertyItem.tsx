import { BoardSpace } from "../../../../../../../types/controllers/BoardSpace";

export const TradePropertyItem:React.FC<{space:BoardSpace}> = ({space}) => {
    console.log(space.property)
    return (
        <div>
            {space.boardSpaceName}
        </div>
    )
}