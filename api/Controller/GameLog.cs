using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/monopoly/gameLog")]
public class GameLogController(IDbConnection db) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<Card>> GetGameLogs(string gameId){

        var gameLogsGetSql = @"
            SELECT * FROM GAMELOG WHERE GameID=@GameId
        ";
        
        var logs = await db.QueryAsync<GameLog>(gameLogsGetSql,new {GameId = gameId });

        return Ok(logs);
    }
}