using System.Data;
using Dapper;

public class PlayerRepository(IDbConnection db): IPlayerRepository
{

    public async Task<Player> GetByIdAsync(string id)
    {
        var query = @"
            SELECT p.*, pi.iconurl
            FROM Player p
            LEFT JOIN PlayerIcon pi ON p.IconId = pi.Id
            WHERE p.Id = @Id
        ";
        return await db.QuerySingleAsync<Player>(query, new { Id = id });
    }

    public async Task<IEnumerable<Player>> GetAllAsync()
    {
        var query = @"
            SELECT p.*, pi.iconurl
            FROM Player p
            LEFT JOIN PlayerIcon pi ON p.IconId = pi.Id
        ";
        var players = await db.QueryAsync<Player>(query);
        return players.AsList();
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

    public async Task<Player> Create(PlayerCreateParams createparams)
    {
        var uuid = Guid.NewGuid().ToString();
        
        var addNewPlayer = @"
            INSERT INTO Player (Id,PlayerName,IconId)
            VALUES (@Id,@PlayerName, @IconId)
        ";

        var parameters = new 
        {
            Id = uuid,
            createparams.PlayerName,
            createparams.IconId,
        };

        await db.ExecuteAsync(addNewPlayer,parameters);

        var newPlayer = await GetByIdAsync(uuid);
        return newPlayer;
    }
}