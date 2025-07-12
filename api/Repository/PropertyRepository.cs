using System.Data;
using api.Entity;
using api.Interface;

namespace api.Repository;
public class PropertyRepository(IDbConnection db) : BaseRepository<Property, int>(db, "Property"), IPropertyRepository
{

}