using System.Data;
using System.Text;
using api.Interface;
using Dapper;

namespace api.Repository;
public class PlayerRepository : BaseRepository<Player, Guid>, IPlayerRepository
{
    public PlayerRepository(IDbConnection db, IPlayerIconRepository playerIconRepository) : base(db, "Player")
    {
        _playerIconRepository = playerIconRepository;
        _playerIconsLazy = new Lazy<Task<List<PlayerIcon>>>(LoadPlayerIconsAsync);
        _db = db;
    }
    public readonly Lazy<Task<List<PlayerIcon>>> _playerIconsLazy;
    public Task<List<PlayerIcon>> PlayerIcons => _playerIconsLazy.Value;

    private readonly IDbConnection _db;
    private readonly IPlayerIconRepository _playerIconRepository;
    public required IPlayerIconRepository PlayerIconRepository { get; init; }
    private async Task<List<PlayerIcon>> LoadPlayerIconsAsync()
    {
        var icons = await _playerIconRepository.GetAllAsync();
        return [.. icons];
    }
    public override async Task<Player> GetByIdAsync(Guid id)
    {
        var query = @"
            SELECT p.*, pi.iconurl
            FROM Player p
            LEFT JOIN PlayerIcon pi ON p.IconId = pi.Id
            WHERE p.Id = @Id
        ";
        return await _db.QuerySingleAsync<Player>(query, new { Id = id });
    }

    public override async Task<IEnumerable<Player>> GetAllAsync()
    {
        var query = @"
            SELECT p.*, pi.iconurl
            FROM Player p
            LEFT JOIN PlayerIcon pi ON p.IconId = pi.Id
            ORDER BY Id
        ";
        var players = await _db.QueryAsync<Player>(query);
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

        var players = await _db.QueryAsync<Player>(sql, parameters);

        if(searchParams.ExcludeId != null)
        {
            return players.Where(p => p.ToString() != searchParams.ExcludeId).AsList();
        }
        return players.AsList();
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
        var result = await _db.ExecuteAsync(sql.ToString(), parameters);
        return result > 0; // Returns the number of rows affected
    }
}