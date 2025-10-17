using System.Data;
using api.Entity;
using Dapper;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/monopoly-app/api/gamecard")]
public class GameCardController(IDbConnection db) : ControllerBase {

    //Handles the logic of keeping a deck in sync while returning a Card in the end
    [HttpGet("getone")]
    public async Task<ActionResult<Card>> GetOne(Guid gameId, int cardTypeId){

        var cardGetSql = @"
            SELECT 
                gc.Id,
                gc.CardId,
                gc.GameId,
                c.Id,
                c.CardActionId,
                c.Amount,
                c.AdvanceToSpaceId,
                c.CardTypeId,
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
                gameCard.Card.CardDescription = themeCard.CardDescription;
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

            //then fetch a new one from the fresh deck
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

        return Ok(singleGameCard.Card);
    }
}