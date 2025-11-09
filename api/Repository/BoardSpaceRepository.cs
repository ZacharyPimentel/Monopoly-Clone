using System.Data;
using api.Entity;
using api.Interface;
using Dapper;
namespace api.Repository;

public class BoardSpaceRepository(IDbConnection db, IGameRepository gameRepository) : BaseRepository<BoardSpace, int>(db, "BoardSpace"), IBoardSpaceRepository
{
    public async Task<List<BoardSpace>> GetAllForGameWithDetailsAsync(Guid gameId)
    {
        Game game = await gameRepository.GetByIdAsync(gameId);

        var sql = @"
            SELECT bs.*, bst.BoardSpaceName, bst.ThemeId
            FROM BoardSpace bs
            LEFT JOIN BoardSpaceTheme bst ON bs.Id = bst.BoardSpaceId
            WHERE bst.ThemeId = @ThemeId;

            SELECT 
                p.Id,
                p.PurchasePrice,
                p.MortgageValue,
                p.upgradeCost,
                p.BoardSpaceId, 
                p.SetNumber,
                gp.Id AS GamePropertyId, 
                gp.PlayerId, 
                gp.UpgradeCount, 
                gp.Mortgaged, 
                gp.GameId,
                tp.ThemeId,
                tp.PropertyId,
                tp.Color
            FROM Property p
            JOIN GameProperty gp ON p.Id = gp.PropertyId AND gp.GameId = @GameId
            LEFT JOIN ThemeProperty tp ON p.Id = tp.PropertyId AND tp.ThemeId = @ThemeId;
            
            SELECT * FROM PropertyRent;
        ";

        var multi = await db.QueryMultipleAsync(sql, new { GameId = gameId, ThemeId = game.ThemeId });
        var boardSpaces = multi.Read<BoardSpace>().ToList();
        var properties = multi.Read<Property>().ToList();
        var propertyRents = multi.Read<PropertyRent>().ToList();
        // Map properties to board spaces
        foreach (var property in properties)
        {
            var boardSpace = boardSpaces.FirstOrDefault(bs => bs.Id == property.BoardSpaceId);
            if (boardSpace != null)
                boardSpace.Property = property;
        }

        // Map property rents to properties
        foreach (var rent in propertyRents)
        {
            var property = properties.FirstOrDefault(p => p.Id == rent.PropertyId);
            property?.PropertyRents.Add(rent);
        }

        return boardSpaces;
    }
}