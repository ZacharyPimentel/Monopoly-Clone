import { useEffect, useRef, useState } from "react"
import { PlayerList } from "../../../playerList/PlayerList"
import { GameStatus } from "./components/GameStatus"
import { Trades } from "./components/Trades"
import { Rules } from "./components/Rules"
import { OwnedProperties } from "./components/OwnedProperties"

export const GameInformation:React.FC<{}> = ({}) => {


    return (
        <div className={`bg-totorodarkgreen flex-1 min-w-[300px] flex flex-col gap-[10px] p-[10px]`}>
            <GameStatus/>
            <PlayerList/>
            <Trades/>
            <Rules/>
            <OwnedProperties/>
        </div>
        
    )
}