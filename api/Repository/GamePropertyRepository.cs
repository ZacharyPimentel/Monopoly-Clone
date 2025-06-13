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
                p.Id,
                p.BoardSpaceId,
                p.PurchasePrice,
                g.Id,
                bst.BoardSpaceName
            FROM 
                GameProperty gp
            JOIN Property p ON p.Id = gp.PropertyId
            JOIN Game g ON gp.GameId = g.Id
            JOIN BoardSpaceTheme bst ON bst.BoardSpaceId = p.BoardSpaceId
            WHERE
                gp.Id = @GamePropertyId
            AND
                gp.GameId = g.Id
        ";
        var result = await db.QueryAsync<GameProperty, Property, Game, BoardSpaceTheme, GameProperty>(
            sql,
            (gp,p,g,bst) =>
            {
                gp.BoardSpaceName = bst.BoardSpaceName;
                gp.BoardSpaceId = p.BoardSpaceId;
                gp.PurchasePrice = p.PurchasePrice;
                return gp;
            },
            new { GamePropertyId = gamePropertyId },
            splitOn:"Id,Id,BoardSpaceName"
        );

        return result.Single();
    }
}