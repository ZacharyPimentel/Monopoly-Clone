import { GameStatus } from "./components/GameStatus"
import { Trades } from "./components/Trades"
import { Rules } from "./components/Rules"
import { OwnedProperties } from "./components/OwnedProperties"
import { PlayerList } from "./components/PlayerList/PlayerList"
import { useGameState } from "../../../../stateProviders/GameStateProvider"

export const GameInformation:React.FC<{}> = ({}) => {
    
    const gameState = useGameState();
    return (
        <div className={`bg-totorodarkgreen flex-1 flex flex-col gap-[10px] p-[10px]`}>
            <GameStatus/>
            <PlayerList/>
            <Rules/>
            {gameState.game?.gameStarted && (<>
                <Trades/>
                <OwnedProperties/>
            </>)}
        </div>
    )
}