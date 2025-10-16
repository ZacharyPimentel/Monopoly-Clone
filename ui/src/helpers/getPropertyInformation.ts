import { Theme } from "../types/stateProviders/GameState";

export const getPropertyInformation = (theme:Theme,_position:number) => {
    //const propertyPositionIndex = setTilePositions[propertySetIndex].indexOf(position);
    const propertyInformation = theme.propertySets[0]?.properties[0];
    return propertyInformation   
}