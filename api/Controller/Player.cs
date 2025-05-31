using api.DTO.Entity;
using api.Interface;
using Microsoft.AspNetCore.Mvc;

namespace api.Controller;
[ApiController]
[Route("/monopoly/player")]
public class PlayerController(IPlayerRepository playerRepository, ICacheService cacheService) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<Card>> Search([FromQuery]PlayerWhereParams whereParams){

        var players = await cacheService.CachedResponse( async () => await playerRepository.SearchWithIconsAsync(whereParams));
        
        return Ok(players);
    }
}