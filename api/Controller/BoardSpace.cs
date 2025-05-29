using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient; // or System.Data.SqlClient
using api.Entity; // or System.Data.SqlClient

[ApiController]
[Route("/monopoly/boardspace")]
public class BoardSpaceController(IDbConnection db) : ControllerBase {

    [HttpGet]
    public async Task<ActionResult<List<BoardSpace>>> GetAllBoardSpaces(){
        var sql = @"
            SELECT * FROM BoardSpace;
            SELECT * FROM Property;
            SELECT * FROM PropertyRent;
        ";
        var multi = await db.QueryMultipleAsync(sql);
        var boardSpaces = multi.Read<BoardSpace>().ToList();
        var properties = multi.Read<Property>().ToList();
        var propertyRents = multi.Read<PropertyRent>().ToList();
        // Map properties to board spaces
        foreach (var property in properties)
        {
            var boardSpace = boardSpaces.FirstOrDefault(bs => bs.Id == property.BoardSpaceId);
            if (boardSpace != null)
                boardSpace.Property = property;
        }

        // Map property rents to properties
        foreach (var rent in propertyRents)
        {
            var property = properties.FirstOrDefault(p => p.Id == rent.PropertyId);
            if (property != null)
            {
                property.PropertyRents.Add(rent);
            }
        }

        return Ok(boardSpaces);
    }
}