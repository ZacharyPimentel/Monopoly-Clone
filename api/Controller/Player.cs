using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient; // or System.Data.SqlClient

[ApiController]
[Route("/monopoly/player")]
public class PlayerController(IDbConnection db,IPlayerRepository playerRepository) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<List<Player>>> SearchPlayers(
        [FromQuery] bool? isActive
    )
    {

        var searchParams = new PlayerSearchParams
        {
            Active = isActive
        };

        var result = await playerRepository.Search(searchParams);

        return Ok(result);
    }
}