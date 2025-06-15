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
                gc.Played,
                c.Id,
                c.CardActionId,
                c.Amount,
                c.AdvanceToSpaceId,
                c.CardTypeId,
                tc.CardDescription
            FROM GameCard gc
            LEFT JOIN Card c ON c.Id = gc.CardId
            LEFT JOIN ThemeCard tc ON tc.CardId = c.Id
            WHERE gc.GameId = @GameId AND c.CardTypeId = @CardTypeId AND gc.Played = FALSE
            ORDER BY RANDOM()
            LIMIT 1
        ";

        var gameCard = await db.QueryAsync<GameCard, Card, ThemeCard, GameCard>(
            cardGetSql,
            (gameCard, card, themeCard) =>
            {
                gameCard.Card = card;
                gameCard.Card.CardDescription = themeCard.CardDescription;
                return gameCard;
            },
            new { CardTypeId = (int)cardTypeId, GameId = gameId },
            splitOn: "Id, CardDescription"
        );

        //if no more cards in the deck, add them all back
        if (gameCard.ToArray().Length == 0)
        {
            var cardsInsertSql = @"
                UPDATE 
                    GameCard
                SET 
                    Played = TRUE,
                    PlayedAt = NULL
                WHERE 
                    GameId = @GameId
                AND 
                    CardTypeId = @CardTypeId
            ";
            await db.ExecuteAsync(cardsInsertSql, new { GameId = gameId, CardTypeId = (int)cardTypeId });

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

        //set the played card to have been played
        var deleteSql = @"
            UPDATE GameCard
            SET 
                Played = true,
                PlayedAt = NOW()
            WHERE Id = @Id
        ";
        await db.ExecuteAsync(deleteSql, new { singleGameCard.Id });

        return validatedGameCard.Card ?? throw new Exception(EnumExtensions.GetEnumDescription(Errors.CardDoesNotExist));
    }

    public async Task<GameCard?> GetLastPlayedGameCard(Guid gameId)
    {
        var sql = @"
            SELECT 
                gc.Id,
                gc.CardId,
                gc.GameId,
                gc.Played
                c.Id,
                c.CardActionId,
                c.Amount,
                c.AdvanceToSpaceId,
                c.CardTypeId,
                tc.CardDescription
            FROM GameCard gc
            LEFT JOIN Card c ON c.Id = gc.CardId
            LEFT JOIN ThemeCard tc ON tc.CardId = c.Id
            WHERE 
                gc.played = true
            AND 
                GameId = @GameId
            ORDER BY 
                PlayedAt DESC
            LIMIT 1
        ";

        GameCard? gameCard = (await db.QueryAsync<GameCard, Card, ThemeCard, GameCard>(
            sql,
            (gameCard, card, themeCard) =>
            {
                gameCard.Card = card;
                gameCard.Card.CardDescription = themeCard.CardDescription;
                return gameCard;
            },
            new { GameId = gameId },
            splitOn: "Id, CardDescription"
        )).FirstOrDefault();

        return gameCard;
    }

}