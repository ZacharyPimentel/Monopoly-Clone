using System.Data;
using Dapper;

public class GameRepository(IDbConnection db) : IGameRepository
{
    public async Task<Game> GetByIdAsync( int id)
    {
        var sql = "SELECT * FROM GAME WHERE Id = @Id";
        var game = await db.QuerySingleAsync<Game>(sql,id);
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