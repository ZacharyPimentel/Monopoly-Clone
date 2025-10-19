using System.Data;
using api.DTO.Entity;
using api.Entity;
using api.Enumerable;
using api.Helper;
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
            WHERE g.Deleted = false";

        var parameters = new DynamicParameters();

        if (searchParams.GameName != null)
        {
            sql += " AND g.GameName = @GameName";
            parameters.Add("GameName", searchParams.GameName);
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
            g.*, COUNT(p.Id) AS ActivePlayerCount,
            COUNT(gp.Password) > 0 AS HasPassword
            FROM Game g
            LEFT JOIN Player p ON g.Id = p.GameId AND p.Active = true
            LEFT JOIN GamePassword gp on gp.GameId = g.Id
            WHERE g.Deleted = false
            GROUP BY g.Id
            ORDER BY g.Id
        ";
        var games = await db.QueryAsync<Game>(sql);
        return games.AsList();
    }
    public async Task<Game> GetByIdWithDetailsAsync(Guid gameId)
    {
        //get the current game, join the current player turn from TurnOrder
        var sql = @"
            WITH FilteredTurnOrder AS (
                SELECT 
                t.PlayerId, t.GameId, t.PlayOrder
                FROM TURNORDER AS t
                WHERE HasPlayed = false 
                AND t.GameId = @Id
                ORDER BY PlayOrder
                LIMIT 1
            )
            SELECT 
                g.*, 
                (
                    SELECT EXISTS (
                        SELECT 1 FROM GamePassword gp WHERE gp.GameId = g.Id
                    )
                ) AS HasPassword,
                f.PlayerId AS CurrentPlayerTurn, 
                ldr.DiceOne, 
                ldr.DiceTwo, 
                ldr.UtilityDiceOne, 
                ldr.UtilityDiceTwo
            FROM Game g
            LEFT JOIN FilteredTurnOrder AS f ON g.Id = f.GameId
            Left JOIN LastDiceRoll ldr ON g.id = ldr.GameId
            WHERE g.Id = @Id
        ";

        Game? game = await db.QuerySingleOrDefaultAsync<Game>(sql, new { Id = gameId });
        if (game == null)
        {
            var errorMessage = EnumExtensions.GetEnumDescription(Errors.GameDoesNotExist);
            throw new Exception(errorMessage);
        }
        return game;
    }

    public async Task AddMoneyToFreeParking(Guid gameId, int amount)
    {
        var sql = @"
            UPDATE
                Game
            SET 
                MoneyInFreeParking = MoneyInFreeParking + @Amount
            WHERE
                Id = @GameId
        ";

        await db.ExecuteAsync(sql, new { GameId = gameId, Amount = amount });
    }
    public async Task PayoutFreeParkingToPlayer(Player player)
    {
        var sql = @"
            UPDATE Player
            SET 
                Money = Money + (
                    SELECT 
                        MoneyInFreeParking
                    FROM 
                        GAME
                    WHERE
                        Id = @GameId
                )
            WHERE Id = @PlayerId

            UPDATE Game
            SET MoneyInFreeParking = 0
            WHERE Id = @GameId
        ";

        await db.ExecuteAsync(sql, new { player.Id, player.GameId });
    }

}