import { GameStatus } from "./components/GameStatus"
import { Trades } from "./components/Trades/Trades"
import { Rules } from "./components/Rules"
import { OwnedProperties } from "./components/OwnedProperties"
import { PlayerList } from "./components/PlayerList/PlayerList"
import { useGameState } from "@stateProviders"
import { GameVolume } from "./components/GameVolume"

export const GameInformation:React.FC<{}> = ({}) => {
    
    const gameState = useGameState(['game']);
    return (
        <div className={`bg-totorodarkgreen flex-1 flex flex-col gap-[10px] p-[10px]`}>
            <GameVolume/>
            <PlayerList/>
            {gameState.game?.gameStarted && (<>
                <Trades/>
                <OwnedProperties/>
            </>)}
            <Rules/>
            <GameStatus/>
        </div>
    )
}