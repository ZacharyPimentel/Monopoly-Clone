using System.Data;
using System.Text;
using api.Interface;
using Dapper;

namespace api.Repository;

public class GamePropertyRepository(IDbConnection db) : BaseRepository<GameProperty, int>(db, "GameProperty"), IGamePropertyRepository
{
    public async Task<bool> CreateForNewGameAsync(Guid gameId)
    {
        var gamePropertySql = @"
            INSERT INTO GameProperty (PropertyId,GameId,CreatedAt)
            SELECT 
                Property.Id AS PropertyId, 
                @GameId AS GameId,
                CURRENT_TIMESTAMP AT TIME ZONE 'UTC' AS CreatedAt
            FROM Property
        ";
        var result = await db.ExecuteAsync(gamePropertySql, new { GameId = gameId });
        return result > 0;
    }
}