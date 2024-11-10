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
            SELECT bs.*, p.*
            FROM BoardSpace as bs
            LEFT JOIN Property AS p ON bs.Id = p.BoardSpaceId
            ORDER BY bs.Id
        ";
        var boardSpaces = await db.QueryAsync<BoardSpace, Property, BoardSpace>(
            sql,
            (boardSpace, property) =>
            {
                boardSpace.Property = property; // Set the related Property object
                return boardSpace;
            }
        );
        return Ok(boardSpaces);
    }
}