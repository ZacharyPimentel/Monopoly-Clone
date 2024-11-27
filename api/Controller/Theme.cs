using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/monopoly/theme")]
public class ThemeController(IDbConnection db) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<List<Theme>>> GetAllThemes(){
        var sql = "SELECT Id, ThemeName FROM Theme";
        var themes = await db.QueryAsync<Theme>(sql);
        return Ok(themes);
    }
}