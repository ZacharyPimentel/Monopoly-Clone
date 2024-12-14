using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/monopoly/gamecard")]
public class GameCardController(IDbConnection db) : ControllerBase {

    [HttpGet("getone")]
    public async Task<ActionResult<GameCard>> GetOne(string gameId, int cardTypeId){

        var cardGetSql = @"
            SELECT 
                gc.*,
                c.*,
                tc.CardDescription
            FROM GameCard gc
            LEFT JOIN Card c ON c.Id = gc.CardId
            LEFT JOIN ThemeCard tc ON tc.CardId = c.Id
            WHERE gc.GameId = @GameId AND c.CardTypeId = @CardTypeId
            ORDER BY RANDOM()
            LIMIT 1
        ";
        
        var gameCard = await db.QueryAsync<GameCard,Card,ThemeCard,GameCard>(
            cardGetSql,
            (gameCard,card,themeCard) => {
                gameCard.Card = card;
                gameCard.CardDescription = themeCard.CardDescription;
                return gameCard;
            },
            new { CardTypeId = cardTypeId, GameId = gameId},
            splitOn:"Id, CardDescription"
        );

        //if no more cards in the deck, add them all back
        if(gameCard.ToArray().Length == 0)
        {
            var cardsInsertSql = @"
                INSERT INTO GAMECARD (CardId,GameId)
                SELECT 
                    Card.Id AS CardId, 
                    @GameId AS GameId
                FROM Card
                WHERE
                    CardTypeId = @CardTypeId
            ";
            await db.ExecuteAsync(cardsInsertSql, new {GameId = gameId, CardTypeId = cardTypeId});

            gameCard = await db.QueryAsync<GameCard,Card,ThemeCard,GameCard>(
                cardGetSql,
                (gameCard,card,themeCard) => {
                    gameCard.Card = card;
                    gameCard.CardDescription = themeCard.CardDescription;
                    return gameCard;
                },
                new { CardTypeId = cardTypeId, GameId = gameId},
                splitOn:"Id, CardDescription"
            );
        }

        var singleGameCard = gameCard.ToArray()[0];

        //delete the card from the deck as it has been played
        var deleteSql = @"
            DELETE FROM GameCard
            WHERE Id = @Id
        ";
        await db.ExecuteAsync(deleteSql, new { Id = singleGameCard.Id});

        return Ok(singleGameCard);
    }
}