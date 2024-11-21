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
    public async Task<IEnumerable<Property>> GetAll()
    {
        return await db.QueryAsync<Property>("SELECT * FROM PROPERTY");
    }
}