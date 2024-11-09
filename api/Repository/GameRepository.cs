using System.Data;
using Dapper;

public class GameRepository(IDbConnection db) : IGameRepository
{
    public async Task<Game> GetByIdAsync( int gameId)
    {
        //get the current game, join the current player turn from TurnOrder
        var sql = @"
            WITH FilteredTurnOrder AS (
                SELECT t.PlayerId, t.GameId, t.PlayOrder
                FROM TURNORDER AS t
                WHERE HasPlayed = false
                ORDER BY PlayOrder
                LIMIT 1
            )
            SELECT g.*, f.PlayerId AS CurrentPlayerTurn
            FROM Game as g
            LEFT JOIN FilteredTurnOrder AS f ON g.Id = f.GameId
                WHERE g.Id = @Id
        ";
        var game = await db.QuerySingleAsync<Game>(sql,gameId);
        return game;
    }
    public async Task<bool> Update(int id,GameUpdateParams updateParams)
    {
        var currentGame = await GetByIdAsync(id) ?? throw new Exception("Game not found");
        
        if(updateParams.InLobby.HasValue)
        {
            currentGame.InLobby = updateParams.InLobby.Value;
        }
        if(updateParams.GameStarted.HasValue)
        {
            currentGame.GameStarted = updateParams.GameStarted.Value;
        }
        if(updateParams.GameOver.HasValue)
        {
            currentGame.GameOver = updateParams.GameOver.Value;
        }
        if(updateParams.StartingMoney.HasValue)
        {
            currentGame.StartingMoney = updateParams.StartingMoney.Value;
        }
        
        var sql = @"
            UPDATE Game
            SET
                InLobby = @Inlobby,
                GameStarted = @GameStarted,
                GameOver = @GameOver,
                StartingMoney = @StartingMoney
            WHERE Id = @Id
        ";

        var parameters = new {
            currentGame.Id,
            currentGame.InLobby,
            currentGame.GameStarted,
            currentGame.GameOver,
            currentGame.StartingMoney,
        };

        var result = await db.ExecuteAsync(sql,parameters);
        return result > 0;
    }
}