using System.Data;
using api.DTO.Entity;
using api.Entity;
using api.Interface;
using Dapper;

namespace api.Repository;
public class GameRepository(IDbConnection db) : BaseRepository<Game, Guid>(db, "Game"), IGameRepository
{
    public async Task<List<Game>> Search(GameWhereParams searchParams)
    {
        var sql = @"
        SELECT 
            g.*, COUNT(p.Id) AS ActivePlayerCount
            FROM Game g
            LEFT JOIN Player p ON g.Id = p.GameId AND p.Active = true
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (searchParams.GameName != null)
        {
            parameters.Add(" AND g.GameName = @GameName", searchParams.GameName);
        }

        sql += @"
            GROUP BY g.Id
            ORDER BY g.Id
        ";

        var games = await db.QueryAsync<Game>(sql, parameters);
        return games.AsList();
    }

    public async Task<List<Game>> GetAllWithPlayerCountAsync()
    {
        var sql = @"
        SELECT 
            g.*, COUNT(p.Id) AS ActivePlayerCount
            FROM Game g
            LEFT JOIN Player p ON g.Id = p.GameId AND p.Active = true
            GROUP BY g.Id
            ORDER BY g.Id
        ";
        var games = await db.QueryAsync<Game>(sql);
        return games.AsList();
    }
    public async Task<Game?> GetByIdWithDetailsAsync(Guid gameId)
    {
        //get the current game, join the current player turn from TurnOrder
        var sql = @"
            WITH FilteredTurnOrder AS (
                SELECT 
                t.PlayerId, t.GameId, t.PlayOrder
                FROM TURNORDER AS t
                WHERE HasPlayed = false
                ORDER BY PlayOrder
                LIMIT 1
            )
            SELECT g.*, f.PlayerId AS CurrentPlayerTurn, ldr.DiceOne, ldr.DiceTwo, ldr.UtilityDiceOne, ldr.UtilityDiceTwo
            FROM Game as g
            LEFT JOIN FilteredTurnOrder AS f ON g.Id = f.GameId
            Left JOIN LastDiceRoll ldr ON g.id = ldr.GameId
                WHERE g.Id = @Id
        ";
        return await db.QuerySingleOrDefaultAsync<Game>(sql, new { Id = gameId });
    }
}