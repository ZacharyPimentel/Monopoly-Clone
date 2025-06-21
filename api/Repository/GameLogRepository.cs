using System.Data;
using api.Entity;
using api.Interface;
using Dapper;

namespace api.Repository;
public class GameLogRepository(IDbConnection db) : BaseRepository<GameLog,int>(db,"GameLog"), IGameLogRepository
{
    public async Task<List<GameLog>> GetAll(Guid gameId)
    {
        var sql = @"
            SELECT * FROM GameLog
            WHERE GameId = @GameId
            ORDER BY CreatedAt DESC
        ";

        var gameLogs = await db.QueryAsync<GameLog>(sql, gameId);
        return gameLogs.ToList();
    }
    public async Task<List<GameLog>> GetLatestFive(Guid gameId)
    {
        var sql = @"
            SELECT * FROM GameLog
            WHERE GameId = @GameId
            ORDER BY CreatedAt DESC
            LIMIT 5
        ";

        var gameLogs = await db.QueryAsync<GameLog>(sql, new { GameId = gameId });
        return gameLogs.ToList();
    }
}