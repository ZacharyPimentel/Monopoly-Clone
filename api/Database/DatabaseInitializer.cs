using System.Data;
using System.Reflection;
using api.Entity;
using Dapper;
using DbUp;
using Newtonsoft.Json;
namespace api.Database;
public class DatabaseInitializer
{
    public static void Initialize(IDbConnection db, string? dbConnectionString, bool resetDatabase)
    {
        if(dbConnectionString is not string connectionString){
            throw new Exception("DB Connection String Missing");
        }

        if (resetDatabase)
        {
            DropAllTables(db);
        }

        ApplyMigrations(connectionString);
    }

    private static void DropAllTables(IDbConnection db)
    {
        // Get all table names
        var tables = db.Query<string>(@"
            SELECT table_name 
            FROM information_schema.tables 
            WHERE table_schema = 'public'
        ");

        // Drop each table
        foreach (var table in tables)
        {
            db.Execute($"DROP TABLE IF EXISTS \"{table}\" CASCADE;");
        }
    }
    private static void ApplyMigrations(string dbConnectionString)
    {
        // Get path relative to the executing assembly
        var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var migrationPath = Path.Combine(assemblyPath!, "Database", "Migrations");
        
        var upgrader = DeployChanges.To
            .PostgresqlDatabase(dbConnectionString)
            .WithScriptsFromFileSystem(migrationPath)
            .LogToConsole()
            .Build();
        var result = upgrader.PerformUpgrade();
        if (!result.Successful)
        {
            throw new Exception("Database Migrations Failed", result.Error);
        }
    }
}