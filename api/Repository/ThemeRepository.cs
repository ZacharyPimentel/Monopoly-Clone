using System.Data;
using api.Entity;
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
}