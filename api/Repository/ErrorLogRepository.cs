using System.Data;
using api.Entity;
using api.Interface;

namespace api.Repository;
public class ErrorLogRepository(IDbConnection db) : BaseRepository<ErrorLog, int>(db, "ErrorLog"), IErrorLogRepository
{
}