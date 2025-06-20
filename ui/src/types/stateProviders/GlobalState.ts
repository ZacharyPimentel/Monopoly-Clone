import { Game } from "@generated/index"
import { ReactNode } from "react"

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
