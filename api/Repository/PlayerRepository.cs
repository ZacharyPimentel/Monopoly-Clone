using System.Data;
using Dapper;

public class PlayerRepository(IDbConnection db): IPlayerRepository
{

    public async Task<Player> GetByIdAsync(string id)
    {
        var query = "SELECT * FROM PLAYER WHERE Id = @Id";
        return await db.QuerySingleOrDefaultAsync<Player>(query, new {Id = id});
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        var query = "SELECT * FROM Player";
        return await db.QueryAsync<Player>(query);
    }
    public async Task<IEnumerable<Player>> Search(PlayerSearchParams searchParams)
    {
        var sql = "SELECT * FROM Player WHERE 1=1"; // 1=1 is a common pattern to simplify dynamic WHERE clauses

        var parameters = new DynamicParameters();

        if (searchParams.Active.HasValue)
        {
            sql += " AND Active = @IsActive";
            parameters.Add("IsActive", searchParams.Active.Value);
        }

        var players = await db.QueryAsync<Player>(sql, parameters);
        return players.AsList();
    }
    public async Task<bool> Update(string playerId , PlayerUpdateParams updateParams)
    {
        var currentPlayer = await GetByIdAsync(playerId) ?? throw new Exception("Player not found");
        
        if(updateParams.Active.HasValue)
        {
            currentPlayer.Active = updateParams.Active.Value;
        }
        if(updateParams.IconId.HasValue)
        {
            currentPlayer.IconId = updateParams.IconId.Value;
        }
        if(updateParams.PlayerName != null)
        {
            currentPlayer.PlayerName = updateParams.PlayerName;
        }
        
        var sql = @"
            UPDATE Player
            SET
                Active = @Active,
                IconId = @IconId,
                PlayerName = @PlayerName
            WHERE Id = @Id
        ";

        var parameters = new {
            currentPlayer.Id,
            currentPlayer.Active,
            currentPlayer.IconId,
            currentPlayer.PlayerName
        };

        var result = await db.ExecuteAsync(sql,parameters);
        return result > 0;
    }
}