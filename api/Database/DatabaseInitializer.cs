using System.Data;
using Dapper;
using Newtonsoft.Json;
public class DatabaseInitializer
{
    public static void Initialize(IDbConnection db)
    {
        DropAllTables(db);
        CreateAllTables(db); 
        SeedCodeTables(db);
    }
    private static void SeedCodeTables(IDbConnection db)
    {
        //board space categories
        var boardSpaceCategoryJsonData = File.ReadAllText("./Database/SeedData/BoardSpaceCategory.json");
        var boardSpaceCategories = JsonConvert.DeserializeObject<List<BoardSpaceCategory>>(boardSpaceCategoryJsonData);
        foreach(var boardSpaceCategory in boardSpaceCategories)
        {
            db.Execute("INSERT INTO BOARDSPACECATEGORY (ID, CategoryName) VALUES (@Id, @CategoryName)",boardSpaceCategory);
        }

        //board spaces
        var boardSpaceJsonData = File.ReadAllText("./Database/SeedData/BoardSpace.json");
        var boardSpaces = JsonConvert.DeserializeObject<List<BoardSpace>>(boardSpaceJsonData);
        
        foreach(var boardSpace in boardSpaces)
        {
            db.Execute("INSERT INTO BOARDSPACE (ID, BoardSpaceCategoryId) VALUES (@Id, @BoardSpaceCategoryId)",boardSpace);
        }

        //properties
        var propertyJsonData = File.ReadAllText("./Database/SeedData/Property.json");
        var properties = JsonConvert.DeserializeObject<List<Property>>(propertyJsonData);
        foreach(var property in properties)
        {
            db.Execute("INSERT INTO PROPERTY (ID, PurchasePrice,MortgageValue,BoardSpaceId,UpgradeCost,SetNumber) VALUES (@Id, @PurchasePrice,@MortgageValue,@BoardSpaceId,@UpgradeCost,@SetNumber)",property);
        }

        //property rents
        var propertyRentJsonData = File.ReadAllText("./Database/SeedData/PropertyRent.json");
        var propertyRents = JsonConvert.DeserializeObject<List<PropertyRent>>(propertyRentJsonData);
        foreach(var rent in propertyRents)
        {
            db.Execute("INSERT INTO PROPERTYRENT (ID, PropertyId, UpgradeNumber,Rent) VALUES (@Id, @PropertyId,@UpgradeNumber,@Rent)",rent);
        }

        //Player Icons
        var playerIconJsonData = File.ReadAllText("./Database/SeedData/PlayerIcon.json");
        var playerIcons = JsonConvert.DeserializeObject<List<PlayerIcon>>(playerIconJsonData);
        foreach(var icon in playerIcons)
        {
            db.Execute("INSERT INTO PLAYERICON (ID, IconUrl) VALUES (@Id, @IconUrl)",icon);
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
            var sql = @"

                CREATE TABLE IF NOT EXISTS GAME(
                    Id Integer PRIMARY KEY,
                    InLobby BOOLEAN DEFAULT true,
                    GameOver BOOLEAN DEFAULT false,
                    GameStarted BOOLEAN DEFAULT false,
                    StartingMoney Integer DEFAULT 1500
                );
                INSERT INTO GAME (ID) values(1);

                CREATE TABLE IF NOT EXISTS PLAYERICON(
                    Id Integer PRIMARY KEY,
                    IconUrl text
                );

                CREATE TABLE IF NOT EXISTS BOARDSPACECATEGORY(
                    Id INTEGER PRIMARY KEY,
                    CategoryName Text
                );

                CREATE TABLE IF NOT EXISTS BOARDSPACE(
                    Id INTEGER PRIMARY KEY,
                    BoardSpaceCategoryId INTEGER,
                    FOREIGN KEY (BoardSpaceCategoryId) REFERENCES BoardSpaceCategory(Id)
                );

                CREATE TABLE IF NOT EXISTS PLAYER(
                    Id TEXT PRIMARY KEY,
                    PlayerName TEXT,
                    Active BOOLEAN DEFAULT true,
                    Money INTEGER,
                    BoardSpaceId Integer DEFAULT 1,
                    FOREIGN KEY (BoardSpaceId) REFERENCES BoardSpace(Id),
                    IconId Integer NULL CHECK (IconId BETWEEN 1 AND 8),
                    InCurrentGame BOOLEAN DEFAULT false,
                    IsReadyToPlay BOOLEAN DEFAULT false,
                    RollCount INTEGER CHECK (RollCount BETWEEN 0 AND 3) DEFAULT 0,
                    TurnComplete BOOLEAN DEFAULT true
                );

                CREATE TABLE IF NOT EXISTS PROPERTY(
                    Id INTEGER PRIMARY KEY,
                    PlayerId TEXT NULL,
                    FOREIGN KEY (PlayerId) REFERENCES PLAYER(Id),
                    SetNumber INTEGER,
                    PurchasePrice INTEGER,
                    MortgageValue INTEGER,
                    BoardSpaceId INTEGER,
                    FOREIGN KEY (BoardSpaceId) REFERENCES BoardSpace(Id),
                    UpgradeCost INTEGER,
                    UpgradeCount INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS PROPERTYRENT(
                    Id INTEGER PRIMARY KEY,
                    PropertyId INTEGER,
                    FOREIGN KEY (PropertyId) REFERENCES Property(Id),
                    UpgradeNumber INTEGER CHECK (UpgradeNumber BETWEEN 0 AND 5),
                    RENT INTEGER
                );

                CREATE TABLE IF NOT EXISTS TURNORDER(
                    Id INTEGER PRIMARY KEY,
                    PlayerId text,
                    FOREIGN KEY (PlayerId) REFERENCES PLAYER(Id),
                    GameId INTEGER,
                    FOREIGN KEY (GameId) REFERENCES GAME(Id),
                    PlayOrder INTEGER,
                    HasPlayed BOOLEAN DEFAULT false
                );

            ";
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