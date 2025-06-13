import { BoardSpace, Player, Game, Trade } from "@generated/index";
import { useGameDispatch } from "@stateProviders/GameStateProvider"
import { useGlobalDispatch } from "@stateProviders/GlobalStateProvider";
import { LobbyGame } from "@types/websocket/Game";
import { GameLog } from "@types/websocket/GameLog";
import { SocketPlayer } from "@types/websocket/Player";
import { useCallback } from "react"
import { useNavigate } from "react-router-dom";
import { toast } from "react-toastify";

export const useWebSocketCallback = () => {

    const gameDispatch = useGameDispatch();
    const globalDispatch = useGlobalDispatch();
    const navigate = useNavigate();
    
    const playerUpdateCallback = useCallback((currentSocketPlayer:SocketPlayer) => {
        gameDispatch({currentSocketPlayer})
    },[])

    const playerUpdateAllCallback = useCallback((players:Player[]) => {
        gameDispatch({players})
    },[]);

    const gameUpdateCallback = useCallback((game:Game| null) => {
        if(!game){
            navigate('/lobby')
            return
        }
        gameDispatch({game, gameId:game.id})
    },[]);
    
    const boardSpaceUpdateCallback = useCallback( (boardSpaces:BoardSpace[]) => 
        gameDispatch({boardSpaces}
    ),[])

    const logUpdateCallback = useCallback ( (gameLogs:GameLog[]) => {
        gameDispatch({gameLogs})
    },[])

    const tradeUpdateCallback = useCallback( (trades:Trade[]) => {
        gameDispatch({trades})
    },[])
    const errorCallback = useCallback( (message:string) => {
        toast(message,{type:'error'})
        navigate('/lobby')
    },[]);

    const gameListCallback = useCallback((games:LobbyGame[]) => globalDispatch({availableGames:games}),[]);
    
    const gameCreateCallback = useCallback((gameId:string) => {
        navigate(`/game/${gameId}`);
    },[]);

    return {
        boardSpaceUpdateCallback,
        errorCallback,
        gameUpdateCallback,
        logUpdateCallback,
        playerUpdateAllCallback,
        playerUpdateCallback,
        tradeUpdateCallback,
        gameListCallback,
        gameCreateCallback
    }
}