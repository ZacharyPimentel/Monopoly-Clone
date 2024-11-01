using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient; // or System.Data.SqlClient

[ApiController]
[Route("/monopoly/playerIcon")]
public class PlayerIconController(IDbConnection db) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<List<Player>>> GetAll()
    {
        var sql = "SELECT * FROM PlayerIcon";
        var playerIcons = await db.QueryAsync<PlayerIcon>(sql);
        return Ok(playerIcons.AsList());
    }
}