using System.Data;
using System.Text;
using Dapper;

public class GamePropertyRepository(IDbConnection db): IGamePropertyRepository
{

    public async Task<GameProperty> GetByIdAsync(int id)
    {
        var query = @"
            SELECT * FROM GameProperty
            WHERE Id = @Id
        ";
        return await db.QuerySingleAsync<GameProperty>(query, new { Id = id });
    }
    public async Task<bool> Update(int id,GamePropertyUpdateParams updateParams)
    {
        GameProperty currentGameProperty = await GetByIdAsync(id) ?? throw new Exception("GameProperty not found");
        
        if(updateParams.PlayerId != null)
        {
            currentGameProperty.PlayerId = updateParams.PlayerId;
        }
        if(updateParams.UpgradeCount.HasValue)
        {
            currentGameProperty.UpgradeCount = updateParams.UpgradeCount.Value;
        }
        if(updateParams.Mortgaged.HasValue)
        {
            currentGameProperty.Mortgaged = updateParams.Mortgaged.Value;
        }
        
        var sql = @"
            UPDATE GameProperty
            SET
                PlayerId = @PlayerId,
                UpgradeCount = @UpgradeCount,
                Mortgaged = @Mortgaged
            WHERE Id = @Id
        ";

        var parameters = new {
            currentGameProperty.Id,
            currentGameProperty.UpgradeCount,
            currentGameProperty.PlayerId,
            currentGameProperty.Mortgaged
        };

        var result = await db.ExecuteAsync(sql,parameters);
        return result > 0;
    }
}