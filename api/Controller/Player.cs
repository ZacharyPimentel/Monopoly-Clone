using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/monopoly/player")]
public class PlayerController(IPlayerRepository playerRepository, ICacheService cacheService) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<Card>> Search([FromQuery]PlayerWhereParams whereParams){

        var players = await cacheService.CachedResponse(() => playerRepository.Search(whereParams));
        
        return Ok(players);
    }
}