import { useGameState } from "@stateProviders/GameStateProvider"

export const useCurrentPlayer = () => {
    const {players,game} = useGameState();
    return players.find( (player) => player.id === game?.currentPlayerTurn);
}
