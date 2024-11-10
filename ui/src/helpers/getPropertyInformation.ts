import { Theme } from "../types/stateProviders/GameState";
import { setTilePositions } from "./tilePositions";

export const getPropertyInformation = (theme:Theme,position:number) => {
    const propertySetIndex = setTilePositions.findIndex( (set) => set.includes(position));
    //const propertyPositionIndex = setTilePositions[propertySetIndex].indexOf(position);
    const propertyInformation = theme.propertySets[0]?.properties[0];
    return propertyInformation   
}