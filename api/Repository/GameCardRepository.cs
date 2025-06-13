using System.Data;
using api.Entity;
using api.Enumerable;
using api.Helper;
using api.Interface;
using Dapper;

namespace api.Repository;

public class GameCardRepository(IDbConnection db) : BaseRepository<GameCard, int>(db, "GameCard"), IGameCardRepository
{
    public async Task<bool> CreateForNewGameAsync(Guid gameId)
    {
        var gameCardSql = @"
            INSERT INTO GAMECARD (CardId,GameId,CreatedAt)
            SELECT 
                Card.Id AS CardId, 
                @GameId AS GameId,
                CURRENT_TIMESTAMP AT TIME ZONE 'UTC' AS CreatedAt
            FROM Card
        ";
        var result = await db.ExecuteAsync(gameCardSql, new { GameId = gameId });
        return result > 0;
    }
    public async Task<Card> PullCardForGame(Guid gameId, CardTypeIds cardTypeId)
    {
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
            new { CardTypeId = (int)cardTypeId, GameId = gameId},
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
            await db.ExecuteAsync(cardsInsertSql, new {GameId = gameId, CardTypeId = (int)cardTypeId});

            //then fetch a new one from the fresh deck
            gameCard = await db.QueryAsync<GameCard, Card, ThemeCard, GameCard>(
                cardGetSql,
                (gameCard, card, themeCard) =>
                {
                    gameCard.Card = card;
                    gameCard.CardDescription = themeCard.CardDescription;
                    return gameCard;
                },
                new { CardTypeId = (int)cardTypeId, GameId = gameId },
                splitOn: "Id, CardDescription"
            );
        }

        var singleGameCard = gameCard.First();

        if (singleGameCard is not GameCard validatedGameCard)
        {
            string errorMessage = EnumExtensions.GetEnumDescription(Errors.GameCardDoesNotExist);
            throw new Exception(errorMessage);
        }

        //delete the card from the deck as it has been played
            var deleteSql = @"
            DELETE FROM GameCard
            WHERE Id = @Id
        ";
        await db.ExecuteAsync(deleteSql, new { singleGameCard.Id});

        return validatedGameCard.Card ?? throw new Exception(EnumExtensions.GetEnumDescription(Errors.CardDoesNotExist));
    }
}