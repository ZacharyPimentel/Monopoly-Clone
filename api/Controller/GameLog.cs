using System.Data;
using api.DTO.Entity;
using api.Interface;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/monopoly-app/api/gameLog")]
public class GameLogController(
    IDbConnection db,
    IGameLogRepository gameLogRepository
) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<Card>> GetGameLogs(Guid gameId){

        var logs = await gameLogRepository.SearchAsync(new GameLogWhereParams
        {
            GameId = gameId
        },
        new{}
        );

        return Ok(logs);
    }
}