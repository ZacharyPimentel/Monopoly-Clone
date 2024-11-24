using System.Data;
using Dapper;

public class GameLogRepository(IDbConnection db): IGameLogRepository
{
    public async Task CreateLog(string gameId, string message)
    {
        
        var insertValues = new GameLog
        {
            GameId = gameId,
            Message = message,
            CreatedAt = DateTime.Now,
        };

        var sql = @"
            INSERT INTO GameLog (GameId, Message, CreatedAt)
            VALUES (@GameId, @Message, @CreatedAt)
        ";
        await db.ExecuteAsync(sql,insertValues);
    }
    public async Task<List<GameLog>> GetAll(string gameId)
    {
        var sql = @"
            SELECT * FROM GameLog
            WHERE GameId = @GameId
            ORDER BY CreatedAt DESC
        ";

        var gameLogs = await db.QueryAsync<GameLog>(sql,gameId);
        return gameLogs.ToList();
    }
    public async Task<List<GameLog>> GetLatestFive(string gameId)
    {
        var sql = @"
            SELECT * FROM GameLog
            WHERE GameId = @GameId
            ORDER BY CreatedAt DESC
            LIMIT 5
        ";

        var gameLogs = await db.QueryAsync<GameLog>(sql,new {GameId = gameId});
        return gameLogs.ToList();
    }
}