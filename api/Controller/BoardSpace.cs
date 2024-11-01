using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient; // or System.Data.SqlClient

[ApiController]
[Route("/monopoly/boardspace")]
public class BoardSpaceController(IDbConnection db) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<List<BoardSpace>>> GetAllBoardSpaces(){
        var sql = @"
            SELECT * FROM BoardSpace
        ";
        var result = await db.QueryAsync(sql);
        return Ok(result);
    }
}