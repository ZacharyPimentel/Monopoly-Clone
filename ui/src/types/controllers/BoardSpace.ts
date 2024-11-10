import { Property } from "./Property"

export type BoardSpace = {
    id:string
    boardSpaceCategoryId:number,
    property: Property | null
}