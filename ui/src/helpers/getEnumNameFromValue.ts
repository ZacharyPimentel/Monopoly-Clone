import { WebSocketEvents } from "@generated"

export const getEnumNameFromValue = (enumValue:number) => {
    return WebSocketEvents[enumValue]
}