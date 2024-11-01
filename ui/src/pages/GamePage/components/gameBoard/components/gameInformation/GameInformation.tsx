import { useEffect, useRef, useState } from "react"
import { GameStatus } from "./components/GameStatus"
import { Trades } from "./components/Trades"
import { Rules } from "./components/Rules"
import { OwnedProperties } from "./components/OwnedProperties"
import { PlayerList } from "../playerList/PlayerList"

export const GameInformation:React.FC<{}> = ({}) => {


    return (
        <div className={`bg-totorodarkgreen flex-1 flex flex-col gap-[10px] p-[10px]`}>
            <GameStatus/>
            <PlayerList/>
            <Trades/>
            <Rules/>
            <OwnedProperties/>
        </div>
        
    )
}