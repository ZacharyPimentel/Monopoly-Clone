using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/monopoly/player")]
public class PlayerController(IDbConnection db,IPlayerRepository playerRepository) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<Card>> Search([FromQuery]PlayerWhereParams whereParams){

        var players = await playerRepository.Search(whereParams);
        
        return Ok(players);
    }
}