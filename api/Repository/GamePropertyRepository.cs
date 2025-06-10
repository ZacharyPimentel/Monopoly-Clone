using System.Data;
using api.Entity;
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

    public async Task<GameProperty> GetByIdWithDetailsAsync(int gamePropertyId)
    {
        var sql = @"
            SELECT
                gp.Id,
                gp.PlayerId,
                gp.GameId,
                gp.UpgradeCount,
                gp.PropertyId,
                gp.Mortgaged,
                p.BoardSpaceId,
                p.PurchasePrice,
                p.Id
            FROM 
                GameProperty gp
            JOIN Property p ON p.Id = gp.PropertyId
            WHERE
                gp.Id = @GamePropertyId
        ";
        GameProperty result = await db.QuerySingleAsync<GameProperty>(sql, new { GamePropertyId = gamePropertyId });
        return result;
    }
}