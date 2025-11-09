using System.Data;
using api.Entity;
using Dapper;
using Newtonsoft.Json;
namespace api.Database;
public class DatabaseInitializer
{

    //Update this prefix if folder structure changes
    private const string PathPrefix = "./Database/SeedData/";

    public static void Initialize(IDbConnection db)
    {
        DropAllTables(db);
        CreateAllTables(db);
        SeedCodeTables(db);
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
        //board space themes
        var boardSpaceThemeJsonData = File.ReadAllText("./Database/SeedData/Themes/Monopoly/BoardSpaceTheme.json");
        var boardSpaceTheme = JsonConvert.DeserializeObject<List<BoardSpaceTheme>>(boardSpaceThemeJsonData);
        foreach(var boardSpace in boardSpaceTheme)
        {
            db.Execute("INSERT INTO BOARDSPACETHEME (ThemeId,BoardSpaceId,BoardSpaceName) VALUES (@ThemeId,@BoardSpaceId,@BoardSpaceName)",boardSpace);
        }

        //theme property
        var themePropertyJsonData = File.ReadAllText("./Database/SeedData/Themes/Monopoly/ThemeProperty.json");
        var themeProperties = JsonConvert.DeserializeObject<List<ThemeProperty>>(themePropertyJsonData);
        
        foreach(var themeProperty in themeProperties)
        {
            db.Execute("INSERT INTO THEMEPROPERTY (ThemeId,PropertyId,Color) VALUES (@ThemeId,@PropertyId,@Color)",themeProperty);
        }

        //theme cards
        var themeCardJsonData = File.ReadAllText("./Database/SeedData/Themes/Monopoly/ThemeCard.json");
        var themeCards = JsonConvert.DeserializeObject<List<ThemeCard>>(themeCardJsonData);
        foreach(var themeCard in themeCards)
        {
            db.Execute("INSERT INTO THEMECARD (CardId,ThemeId,CardDescription) VALUES (@CardId, @ThemeId,@CardDescription)",themeCard);
        }

        //color groups
        var colorGroupJsonData = File.ReadAllText("./Database/SeedData/ColorGroup.json");
        var colorGroups = JsonConvert.DeserializeObject<List<ColorGroup>>(colorGroupJsonData);
        foreach(var group in colorGroups)
        {
            db.Execute("INSERT INTO COLORGROUP (Id,GroupName) VALUES (@Id, @GroupName)",group);
        }

        //theme colors
        var themeColorJsonData = File.ReadAllText("./Database/SeedData/Themes/Monopoly/ThemeColor.json");
        var themeColors = JsonConvert.DeserializeObject<List<ThemeColor>>(themeColorJsonData);
        foreach(var themeColor in themeColors)
        {
            db.Execute("INSERT INTO ThemeColor (ThemeId,ColorGroupId,Color,Shade) VALUES (@ThemeId,@ColorGroupId,@Color,@Shade)",themeColor);
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
}