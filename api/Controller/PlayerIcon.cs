using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using api.Entity; // or System.Data.SqlClient

[ApiController]
[Route("/monopoly/playerIcon")]
public class PlayerIconController(IDbConnection db, ICacheService cacheService) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<List<Player>>> GetAll()
    {
        var sql = "SELECT * FROM PlayerIcon";
        var playerIcons = await cacheService.CachedResponse(() => db.QueryAsync<PlayerIcon>(sql));
        return Ok(playerIcons.AsList());
    }
}