import { PropertyRent } from "./PropertyRent"

export type Property = {
    id:number
    playerId:string | null
    purchasePrice:number
    mortgageValue:number
    boardSpaceId:number
    upgradeCost:number
    upgradeCount:number
    setNumber:number
    propertyRents:PropertyRent[]
    mortgaged:boolean
    color:string
}

export type PropertyUpdateParams = {
    upgradeCount:number,
    playerId:string
    mortgaged:boolean
}