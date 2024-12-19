import { useState } from "react"
import { ActionButtons } from "../../../../../../../globalComponents/GlobalModal/ActionButtons"
import { usePlayer } from "../../../../../../../hooks/usePlayer"
import { useGameState } from "../../../../../../../stateProviders/GameStateProvider"
import { OptionSelectMenu } from "../../../../../../../globalComponents/FormElements/OptionSelectMenu"
import { useApi } from "../../../../../../../hooks/useApi"

export const CreateTradeModal = () => {

    const gameState = useGameState()
    const {player} = usePlayer()
    const [tradingPlayerId,setTradingPlayerId] = useState('');
    const api = useApi();

    return (
        <div className='flex flex-col gap-[20px]'>
            <p>Create a Trade</p>
            <label>
                <p>Who would you like to trade with?</p>
                <OptionSelectMenu
                    setStateCallback={setTradingPlayerId}
                    apiCall={async() => await api.player.search({
                        excludeId:player.id,
                        inCurrentGame:true
                    })}
                    displayKey="playerName"
                />
                <select onChange={(e) => setTradingPlayerId(e.target.value)}>
                    <option value=''>Select A Player</option>
                    {gameState.players.filter((gamePlayer) => gamePlayer.id !== player?.id ).map( (player) => {
                        return (
                            <option value={player.id}>{player.playerName}</option>
                        )
                    })}
                </select>
            </label>
            <p>What would you like to trade?</p>
            <div className='grid grid-cols-2'>
                <div>
                    <p>Test</p>
                </div>
                <div>
                    <p>Test</p>
                </div>
            </div>
            <ActionButtons
                confirmButtonStyle="success"
                confirmCallback={async() => {}}
                confirmButtonText="Create Trade"
                confirmDisabled={!tradingPlayerId}
            />
        </div>
    )
}