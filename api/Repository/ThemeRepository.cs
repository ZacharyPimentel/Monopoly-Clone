using System.Data;
using Dapper;

public class ThemeRepository(IDbConnection db)
{
    async Task<List<BoardSpace>> GetBoardSpaces(int themeId)
    {
        var sql = @"
            SELECT bs.* FROM BOARDSPACE, bst.BoardSpaceName, bst.ThemeId
            LEFT JOIN BoardSpaceTheme bst ON bs.Id = bst.BoardSpaceId
            WHERE bst.ThemeId = @ThemeId
        ";

        var boardSpaces = await db.QueryAsync<BoardSpace>(sql, new {ThemeId = themeId});
        return boardSpaces.ToList();
    }
}