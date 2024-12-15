using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/monopoly/card")]
public class CardController(IDbConnection db) : ControllerBase {

    [HttpGet("{id}")]
    public async Task<ActionResult<Card>> GetById(int id){

        var cardGetSql = @"
            SELECT 
                c.*,
                tc.CardDescription
            FROM Card c
            LEFT JOIN ThemeCard tc ON tc.CardId = c.Id
            WHERE c.Id = @CardId
            LIMIT 1
        ";
        
        var card = await db.QuerySingleAsync<Card>(cardGetSql,new {CardId = id });

        return Ok(card);
    }
}