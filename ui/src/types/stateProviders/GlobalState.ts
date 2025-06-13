import { ReactNode } from "react"
import { Game } from "../controllers/Game"
import { LobbyGame } from "@types/websocket/Game"

export type GlobalState = {
    modalOpen:boolean
    modalContent: ReactNode | null
    toastOpen:boolean,
    toastStyle:'success' | 'info' | 'error',
    toastMessages:string[],
    ws: signalR.HubConnection
    availableGames:LobbyGame[]
    socketConnected:boolean
}
