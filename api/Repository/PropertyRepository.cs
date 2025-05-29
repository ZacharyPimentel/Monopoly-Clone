using System.Data;
using System.Text;
using api.Entity;
using api.Repository;
using Dapper;

namespace api.Repository;
public class PropertyRepository(IDbConnection db) : BaseRepository<Property, int>(db, "Property"), IPropertyRepository
{

}