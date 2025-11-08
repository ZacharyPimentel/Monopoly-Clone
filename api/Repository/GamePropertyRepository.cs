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
                p.MortgageValue,
                p.SetNumber,
                p.UpgradeCost,
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
            (gp, p, g, bst) =>
            {
                gp.BoardSpaceName = bst.BoardSpaceName;
                gp.BoardSpaceId = p.BoardSpaceId;
                gp.MortgageValue = p.MortgageValue;
                gp.PurchasePrice = p.PurchasePrice;
                gp.UpgradeCost = p.UpgradeCost;
                gp.SetNumber = p.SetNumber;
                return gp;
            },
            new { GamePropertyId = gamePropertyId },
            splitOn: "Id,Id,BoardSpaceName"
        );

        return result.Single();
    }

    public async Task<IEnumerable<GameProperty>> GetBySetNumberWithDetails(Guid gameId,int? setNumber)
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
                p.MortgageValue,
                p.SetNumber,
                p.UpgradeCost,
                g.Id,
                bst.BoardSpaceName
            FROM 
                GameProperty gp
            JOIN Property p ON p.Id = gp.PropertyId
            JOIN Game g ON gp.GameId = g.Id
            JOIN BoardSpaceTheme bst ON bst.BoardSpaceId = p.BoardSpaceId
            WHERE
                p.SetNumber = @SetNumber
            AND
                gp.GameId = @GameId
        ";
        var result = await db.QueryAsync<GameProperty, Property, Game, BoardSpaceTheme, GameProperty>(
            sql,
            (gp, p, g, bst) =>
            {
                gp.BoardSpaceName = bst.BoardSpaceName;
                gp.BoardSpaceId = p.BoardSpaceId;
                gp.MortgageValue = p.MortgageValue;
                gp.PurchasePrice = p.PurchasePrice;
                gp.UpgradeCost = p.UpgradeCost;
                gp.SetNumber = p.SetNumber;
                return gp;
            },
            new { 
                GameId = gameId,
                SetNumber = setNumber 
            },
            splitOn: "Id,Id,BoardSpaceName"
        );

        return result;
    }


    public async Task<bool> AssignAllToPlayer(Guid GameId, Guid PlayerId)
    {
        var sql = @"
            UPDATE GameProperty
            SET PlayerId = @PlayerId
            WHERE GameId = @GameId
        ";
        var result = await db.ExecuteAsync(sql, new { GameId, PlayerId });
        return result > 0;
    }

    public async Task<bool> UnassignAllFromPlayer(Guid GameId, Guid PlayerId)
    {
        var sql = @"
            UPDATE GameProperty
            SET 
                PlayerId = NULL,
                UpgradeCount = 0
            WHERE 
                GameId = @GameId
            AND
                PlayerId = @PlayerId

        ";
        var result = await db.ExecuteAsync(sql, new { GameId, PlayerId });
        return result > 0;
    }
}