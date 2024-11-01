import { Theme } from "../types/GameState";
import { setTilePositions } from "./tilePositions";

export const getPropertyInformation = (theme:Theme,position:number) => {
    const propertySetIndex = setTilePositions.findIndex( (set) => set.includes(position));
    const propertyPositionIndex = setTilePositions[propertySetIndex].indexOf(position);
    const propertyInformation = theme.propertySets[propertySetIndex]?.properties[propertyPositionIndex];
    return propertyInformation   
}