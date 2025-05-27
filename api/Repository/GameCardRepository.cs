using System.Data;
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
}