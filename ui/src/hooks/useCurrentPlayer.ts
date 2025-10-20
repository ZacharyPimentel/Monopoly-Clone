import { useGameState } from "@stateProviders";

export const useCurrentPlayer = () => {
    const {players,game} = useGameState(['players','game']);
    return players.find( (player) => player.id === game?.currentPlayerTurn);
}
