using System.Data;
using System.Text;
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
            ORDER BY Id
        ";
        var players = await db.QueryAsync<Player>(query);
        return players.AsList();
    }
    public async Task<IEnumerable<Player>> Search(PlayerWhereParams searchParams)
    {
        var sql = @"
            SELECT p.*, pi.iconurl 
            FROM Player p
            LEFT JOIN PlayerIcon pi ON p.IconId = pi.Id
            WHERE 1=1
        ";

        var parameters = new DynamicParameters();

        if (searchParams.Active.HasValue)
        {
            sql += " AND Active = @IsActive";
            parameters.Add("IsActive", searchParams.Active.Value);
        }
        if(searchParams.GameId != null)
        {
            sql += " AND GameId = @GameId";
            parameters.Add("GameId", searchParams.GameId);
        }
        
        sql += " ORDER BY Id";

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
        if(updateParams.IsReadyToPlay.HasValue)
        {
            currentPlayer.IsReadyToPlay = updateParams.IsReadyToPlay.Value;
        }
        if(updateParams.BoardSpaceId.HasValue)
        {
            currentPlayer.BoardSpaceId = updateParams.BoardSpaceId.Value;
        }
        if(updateParams.RollCount.HasValue)
        {
            currentPlayer.RollCount = updateParams.RollCount.Value;
        }
        if(updateParams.Money.HasValue)
        {
            currentPlayer.Money = updateParams.Money.Value;
        }
        if(updateParams.TurnComplete.HasValue)
        {
            currentPlayer.TurnComplete = updateParams.TurnComplete.Value;
        }
        if(updateParams.InJail.HasValue)
        {
            currentPlayer.InJail = updateParams.InJail.Value;
        }
        if(updateParams.RollingForUtilities.HasValue)
        {
            currentPlayer.RollingForUtilities = updateParams.RollingForUtilities.Value;
        }
        if(updateParams.JailTurnCount.HasValue)
        {
            currentPlayer.JailTurnCount = updateParams.JailTurnCount.Value;
        }
        
        var sql = @"
            UPDATE Player
            SET
                Active = @Active,
                IconId = @IconId,
                PlayerName = @PlayerName,
                IsReadyToPlay = @IsReadyToPlay,
                BoardSpaceId = @BoardSpaceId,
                RollCount = @RollCount,
                Money = @Money,
                TurnComplete = @TurnComplete,
                InJail = @InJail,
                RollingForUtilities = @RollingForUtilities,
                JailTurnCount = @JailTurnCount
            WHERE Id = @Id
        ";

        var parameters = new {
            currentPlayer.Id,
            currentPlayer.Active,
            currentPlayer.IconId,
            currentPlayer.PlayerName,
            currentPlayer.IsReadyToPlay,
            currentPlayer.BoardSpaceId,
            currentPlayer.RollCount,
            currentPlayer.Money,
            currentPlayer.TurnComplete,
            currentPlayer.InJail,
            currentPlayer.RollingForUtilities,
            currentPlayer.JailTurnCount
        };

        var result = await db.ExecuteAsync(sql,parameters);
        return result > 0;
    }

    public async Task<bool> UpdateMany(PlayerWhereParams whereParams, PlayerUpdateParams updateParams)
    {
        // Start building the SQL query
        var sql = new StringBuilder("UPDATE Player SET ");

        // Dynamically build the SET clause from the updateParams
        var updateClauses = new List<string>();
        foreach (var property in updateParams.GetType().GetProperties())
        {
            if(property.GetValue(updateParams)  != null){
                updateClauses.Add($"{property.Name} = @{property.Name}");
            }
        }
        sql.Append(string.Join(", ", updateClauses));

        // Build the WHERE clause from whereParams if it exists
        var whereClauses = new List<string>();
        foreach (var property in whereParams.GetType().GetProperties())
        {
            if (property.GetValue(whereParams) != null)
            {
                whereClauses.Add($"{property.Name} = @Where{property.Name}");
            }
        }

        if(whereClauses.Count > 0)
        {
            sql.Append(" WHERE ");
            sql.Append(string.Join(" AND ", whereClauses));
        }
        
        // Combine the update and where parameters into a single anonymous object
        var parameters = new DynamicParameters();

        // Add the update parameters
        foreach (var property in updateParams.GetType().GetProperties())
        {
            if(property.GetValue(updateParams)  != null)
            {
                parameters.Add($"@{property.Name}", property.GetValue(updateParams));
            }
        }

        // Add the where parameters with a prefix to avoid conflicts
        foreach (var property in whereParams.GetType().GetProperties())
        {
            parameters.Add($"@Where{property.Name}", property.GetValue(whereParams));
        }

        Console.WriteLine(sql);

        // Execute the update
        var result = await db.ExecuteAsync(sql.ToString(), parameters);
        return result > 0; // Returns the number of rows affected
    }

    public async Task<Player> Create(PlayerCreateParams createparams)
    {
        var uuid = Guid.NewGuid().ToString();
        
        var addNewPlayer = @"
            INSERT INTO Player (Id,PlayerName,IconId,GameId)
            VALUES (@Id, @PlayerName, @IconId, @GameId)
        ";

        var parameters = new 
        {
            Id = uuid,
            createparams.PlayerName,
            createparams.IconId,
            createparams.GameId
        };

        await db.ExecuteAsync(addNewPlayer,parameters);

        var newPlayer = await GetByIdAsync(uuid);
        return newPlayer;
    }
}