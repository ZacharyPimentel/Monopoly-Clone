import { Property } from "./Property"

export type BoardSpace = {
    id:number
    boardSpaceCategoryId:number,
    property: Property | null
    boardSpaceName:string
}