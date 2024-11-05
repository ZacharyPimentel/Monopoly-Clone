using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

class GameState<T> where T : Hub{
    public ConcurrentDictionary<string, HubCallerContext> All { get; } = new();
    public List<SocketPlayer> Players {get;set;} = [];
    public int[]? LastDiceRoll = null;

    public SocketPlayer GetPlayer(string playerID){
        for(int i=0 ; i<Players.Count ; i++){
            if(Players[i].SocketId == playerID){
                return Players[i];
            }
        }
        return new SocketPlayer{SocketId = playerID};
    }
    public void RemovePlayer(string playerID){
        for(int i=0 ; i<Players.Count ; i++){
            if(Players[i].SocketId == playerID){
                Players.RemoveAt(i);
            }
        }
    }

    public void SetLastDiceRoll(int[] rolls)
    {
        LastDiceRoll = rolls;
    }
}