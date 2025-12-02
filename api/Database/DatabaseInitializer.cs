using System.Data;
using System.Reflection;
using api.Entity;
using Dapper;
using DbUp;
using Newtonsoft.Json;
namespace api.Database;
public class DatabaseInitializer
{

    //Update this prefix if folder structure changes
    private const string PathPrefix = "./Database/SeedData/";

    public static void Initialize(IDbConnection db, string? dbConnectionString, bool resetDatabase)
    {
        if (resetDatabase)
        {
            DropAllTables(db);
            CreateAllTables(db);
            SeedCodeTables(db);
        }
        else
        {
            if(dbConnectionString is string connectionString){
                ApplyMigrations(connectionString);
            }
            else
            {
                throw new Exception("DB Connection String Missing");
            }
        }
        
    }

    private static void SeedCodeTables(IDbConnection db)
    {
        var tableNames = new[]{
            "BoardSpaceCategory",
            "BoardSpace",
            "Theme",
            "Property",
            "PropertyRent",
            "PlayerIcon",
            "CardAction",
            "CardType",
            "Card",
        };
        var seedDataPaths = tableNames
            .Select(p => PathPrefix + p + ".sql")
            .ToArray();

        foreach (var seedDataPath in seedDataPaths)
        {
            var sql = File.ReadAllText(seedDataPath);
            db.Execute(sql);
        }

        //color groups
        var colorGroupJsonData = File.ReadAllText("./Database/SeedData/ColorGroup.json");
        var colorGroups = JsonConvert.DeserializeObject<List<ColorGroup>>(colorGroupJsonData)!;
        foreach(var group in colorGroups)
        {
            db.Execute("INSERT INTO COLORGROUP (Id,GroupName) VALUES (@Id, @GroupName)",group);
        }

        // Load all board space theme files across theme folders
        var themeDirs = Directory.GetDirectories("./Database/SeedData/Themes");

        foreach (var themeDir in themeDirs)
        {
            //board space themes
            var bstFilePath = Path.Combine(themeDir, "BoardSpaceTheme.json");
            var boardSpaceThemeJsonData = File.ReadAllText(bstFilePath);
            var boardSpaceThemes = JsonConvert.DeserializeObject<List<BoardSpaceTheme>>(boardSpaceThemeJsonData)!;
            foreach (var boardSpaceTheme in boardSpaceThemes)
            {
                db.Execute("INSERT INTO BOARDSPACETHEME (ThemeId,BoardSpaceId,BoardSpaceName) VALUES (@ThemeId,@BoardSpaceId,@BoardSpaceName)", boardSpaceTheme);
            }
            //theme properties
            var tpFilePath = Path.Combine(themeDir, "ThemeProperty.json");
            var themePropertyJsonData = File.ReadAllText(tpFilePath);
            var themeProperties = JsonConvert.DeserializeObject<List<ThemeProperty>>(themePropertyJsonData)!;

            foreach (var themeProperty in themeProperties)
            {
                db.Execute("INSERT INTO THEMEPROPERTY (ThemeId,PropertyId,Color) VALUES (@ThemeId,@PropertyId,@Color)", themeProperty);
            }
            //theme cards
            var themeCardFilePath = Path.Combine(themeDir, "ThemeCard.json");
            var themeCardJsonData = File.ReadAllText(themeCardFilePath);
            var themeCards = JsonConvert.DeserializeObject<List<ThemeCard>>(themeCardJsonData)!;
            foreach (var themeCard in themeCards)
            {
                db.Execute("INSERT INTO THEMECARD (CardId,ThemeId,CardDescription) VALUES (@CardId, @ThemeId,@CardDescription)", themeCard);
            }
            //theme colors
            var themeColorFilePath = Path.Combine(themeDir, "ThemeColor.json");
            var themeColorJsonData = File.ReadAllText(themeColorFilePath);
            var themeColors = JsonConvert.DeserializeObject<List<ThemeColor>>(themeColorJsonData)!;
            foreach(var themeColor in themeColors)
            {
                db.Execute("INSERT INTO ThemeColor (ThemeId,ColorGroupId,Color,Shade) VALUES (@ThemeId,@ColorGroupId,@Color,@Shade)",themeColor);
            }
        }        
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
    private static void CreateAllTables(IDbConnection db)
    {
        var transaction = db.BeginTransaction();

        try
        {
            var sql = File.ReadAllText(PathPrefix + "Tables/Tables.sql");
            db.Execute(sql,transaction:transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
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