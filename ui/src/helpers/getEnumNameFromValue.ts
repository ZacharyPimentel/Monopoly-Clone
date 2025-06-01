import { WebSocketEvents } from "@generated/WebSocketEvents"

export const getEnumNameFromValue = (enumValue:number) => {
    return WebSocketEvents[enumValue]
}