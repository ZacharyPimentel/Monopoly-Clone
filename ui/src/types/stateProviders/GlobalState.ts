import { ReactNode } from "react"
import { Game } from "../controllers/Game"

export type GlobalState = {
    modalOpen:boolean
    modalContent: ReactNode | null
    toastOpen:boolean,
    toastStyle:'success' | 'info' | 'error',
    toastMessages:string[],
    ws: signalR.HubConnection
    availableGames:Game[]
    socketConnected:boolean
}
