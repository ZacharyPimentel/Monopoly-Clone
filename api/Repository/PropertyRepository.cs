using System.Data;
using System.Text;
using Dapper;

public class PropertyRepository(IDbConnection db): IPropertyRepository
{

    public async Task<Property> GetByIdAsync(int id)
    {
        var query = @"
            SELECT * FROM Property
            WHERE Id = @Id
        ";
        return await db.QuerySingleAsync<Property>(query, new { Id = id });
    }

    public async Task<bool> Update(int id,PropertyUpdateParams updateParams)
    {
        Property currentProperty = await GetByIdAsync(id) ?? throw new Exception("Property not found");
        
        if(updateParams.PlayerId != null)
        {
            currentProperty.PlayerId = updateParams.PlayerId;
        }
        if(updateParams.UpgradeCount.HasValue)
        {
            currentProperty.UpgradeCount = updateParams.UpgradeCount.Value;
        }
        if(updateParams.Mortgaged.HasValue)
        {
            currentProperty.Mortgaged = updateParams.Mortgaged.Value;
        }
        
        var sql = @"
            UPDATE Property
            SET
                PlayerId = @PlayerId,
                UpgradeCount = @UpgradeCount,
                Mortgaged = @Mortgaged
            WHERE Id = @Id
        ";

        var parameters = new {
            currentProperty.Id,
            currentProperty.UpgradeCount,
            currentProperty.PlayerId,
            currentProperty.Mortgaged
        };

        var result = await db.ExecuteAsync(sql,parameters);
        return result > 0;
    }
}