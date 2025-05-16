using System.Data;
using Dapper;

public class ThemeRepository(IDbConnection db) : IThemeRepository
{
    public async Task<List<BoardSpace>> GetBoardSpaces(int themeId)
    {
        var sql = @"
            SELECT bs.* FROM BOARDSPACE, bst.BoardSpaceName, bst.ThemeId
            LEFT JOIN BoardSpaceTheme bst ON bs.Id = bst.BoardSpaceId
            WHERE bst.ThemeId = @ThemeId
        ";

        var boardSpaces = await db.QueryAsync<BoardSpace>(sql, new {ThemeId = themeId});
        return boardSpaces.ToList();
    }
    public async Task<List<Theme>> GetAll()
    {
        var sql = "SELECT Id, ThemeName FROM Theme";
        var themes = await db.QueryAsync<Theme>(sql);
        Console.WriteLine(themes.ToList());
        return themes.ToList();
    }
    public async Task<List<ThemeColor>> GetColors(int themeId)
    {
        var primaryColorsql = @"
            SELECT 
                *
            FROM 
                ThemeColor
            WHERE
                ThemeId = @ThemeId
            AND
                ColorGroupId = 1
            ORDER BY Shade ASC
        ";

        var primaryThemeColors = await db.QueryAsync<ThemeColor>(primaryColorsql, new {ThemeId = themeId});
        return primaryThemeColors.ToList();
    }
}