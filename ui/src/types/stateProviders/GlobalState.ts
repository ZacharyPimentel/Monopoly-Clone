import { ReactNode } from "react"

export type GlobalState = {
    modalOpen:boolean
    modalContent: ReactNode | null
    toastOpen:boolean,
    toastStyle:'success' | 'info' | 'error',
    toastMessages:string[],
}
