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

        //themes
        db.Execute("INSERT INTO THEME (ID,ThemeName) values (@Id,@ThemeName)", new {Id = 1, ThemeName = "Monopoly"});

        //board space themes
        var boardSpaceThemeJsonData = File.ReadAllText("./Database/SeedData/Themes/Monopoly/BoardSpaceTheme.json");
        var boardSpaceTheme = JsonConvert.DeserializeObject<List<BoardSpaceTheme>>(boardSpaceThemeJsonData);
        foreach(var boardSpace in boardSpaceTheme)
        {
            db.Execute("INSERT INTO BOARDSPACETHEME (ID, ThemeId,BoardSpaceId,BoardSpaceName) VALUES (@Id, @ThemeId,@BoardSpaceId,@BoardSpaceName)",boardSpace);
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

        //Card Actions
        var cardActionJsonData = File.ReadAllText("./Database/SeedData/CardAction.json");
        var cardActions = JsonConvert.DeserializeObject<List<CardAction>>(cardActionJsonData);
        foreach(var action in cardActions)
        {
            db.Execute("INSERT INTO CARDACTION (ID, CardActionName) VALUES (@Id, @CardActionName)",action);
        }

        //Card Types
        var cardTypeJsonData = File.ReadAllText("./Database/SeedData/CardType.json");
        var cardTypes = JsonConvert.DeserializeObject<List<CardType>>(cardTypeJsonData);
        foreach(var type in cardTypes)
        {
            db.Execute("INSERT INTO CARDTYPE (ID, CardTypeName) VALUES (@Id, @CardTypeName)",type);
        }

        //Cards
        var cardJsonData = File.ReadAllText("./Database/SeedData/Card.json");
        var cards = JsonConvert.DeserializeObject<List<Card>>(cardJsonData);
        foreach(var card in cards)
        {
            db.Execute("INSERT INTO CARD (ID, AdvanceToSpaceId,Amount,CardTypeId,CardActionId) VALUES (@Id, @AdvanceToSpaceId,@Amount,@CardTypeId,@CardActionId)",card);
        }

        //theme cards
        var themeCardJsonData = File.ReadAllText("./Database/SeedData/Themes/Monopoly/ThemeCard.json");
        var themeCards = JsonConvert.DeserializeObject<List<ThemeCard>>(themeCardJsonData);
        foreach(var themeCard in themeCards)
        {
            db.Execute("INSERT INTO THEMECARD (CardId,ThemeId,CardDescription) VALUES (@CardId, @ThemeId,@CardDescription)",themeCard);
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

                CREATE TABLE IF NOT EXISTS THEME(
                    Id INTEGER PRIMARY KEY,
                    ThemeName TEXT,
                    PrimaryColor TEXT,
                    SecondaryColor TEXT
                );

                CREATE TABLE IF NOT EXISTS GAME(
                    Id TEXT PRIMARY KEY,
                    GameName TEXT UNIQUE,
                    InLobby BOOLEAN DEFAULT true,
                    GameOver BOOLEAN DEFAULT false,
                    GameStarted BOOLEAN DEFAULT false,
                    StartingMoney INTEGER DEFAULT 1500,
                    ThemeId INTEGER NOT NULL,
                    FOREIGN KEY (ThemeId) REFERENCES THEME(Id)
                );

                CREATE TABLE IF NOT EXISTS LASTDICEROLL(
                    Id SERIAL PRIMARY KEY,
                    GameId TEXT,
                    FOREIGN KEY (GameId) REFERENCES GAME(Id),
                    DiceOne INTEGER DEFAULT 1,
                    DiceTwo INTEGER DEFAULT 1
                );

                CREATE TABLE IF NOT EXISTS PLAYERICON(
                    Id INTEGER PRIMARY KEY,
                    IconUrl TEXT
                );

                CREATE TABLE IF NOT EXISTS BOARDSPACECATEGORY(
                    Id INTEGER PRIMARY KEY,
                    CategoryName TEXT
                );

                CREATE TABLE IF NOT EXISTS BOARDSPACE(
                    Id INTEGER PRIMARY KEY,
                    BoardSpaceCategoryId INTEGER,
                    FOREIGN KEY (BoardSpaceCategoryId) REFERENCES BoardSpaceCategory(Id)
                );

                CREATE TABLE IF NOT EXISTS BOARDSPACETHEME(
                    ID Integer PRIMARY KEY,
                    ThemeId INTEGER,
                    FOREIGN KEY (ThemeId) REFERENCES Theme(Id),
                    BoardSpaceId INTEGER,
                    FOREIGN KEY (BoardSpaceId) REFERENCES BoardSpace(Id),
                    BoardSpaceName Text
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
                    TurnComplete BOOLEAN DEFAULT false,
                    InJail BOOLEAN DEFAULT false,
                    GameId TEXT,
                    FOREIGN KEY (GameId) REFERENCES Game(Id),
                    RollingForUtilities BOOLEAN DEFAULT false,
                    JailTurnCount INTEGER DEFAULT 0
                );

                CREATE TABLE IF NOT EXISTS PROPERTY(
                    Id INTEGER PRIMARY KEY,
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
                    Id TEXT PRIMARY KEY,
                    PlayerId TEXT,
                    FOREIGN KEY (PlayerId) REFERENCES PLAYER(Id),
                    GameId TEXT,
                    FOREIGN KEY (GameId) REFERENCES GAME(Id),
                    PlayOrder INTEGER,
                    HasPlayed BOOLEAN DEFAULT false
                );

                CREATE TABLE IF NOT EXISTS GAMEPROPERTY(
                    Id SERIAL PRIMARY KEY,
                    PlayerId TEXT,
                    FOREIGN KEY (PlayerId) REFERENCES Player(Id),
                    GameId TEXT,
                    FOREIGN KEY (GameId) REFERENCES Game(Id),
                    PropertyId INTEGER,
                    FOREIGN KEY (PropertyId) REFERENCES Property(Id),
                    UpgradeCount INTEGER CHECK (UpgradeCount BETWEEN 0 AND 5) DEFAULT 0,
                    Mortgaged BOOLEAN DEFAULT false
                );

                CREATE TABLE IF NOT EXISTS GAMELOG(
                    Id SERIAL PRIMARY KEY,
                    GameId TEXT NOT NULL,
                    FOREIGN KEY (GameId) REFERENCES Game(Id),
                    Message TEXT NOT NULL,
                    CreatedAt TIMESTAMP NOT NULL
                );
                
                CREATE TABLE IF NOT EXISTS CARDACTION(
                    Id INTEGER PRIMARY KEY,
                    CardActionName TEXT
                );

                CREATE TABLE IF NOT EXISTS CARDTYPE(
                    Id INTEGER PRIMARY KEY,
                    CardTypeName TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS CARD(
                    Id INTEGER PRIMARY KEY,
                    CardActionId INTEGER,
                    FOREIGN KEY (CardActionId) REFERENCES CARDACTION(Id),
                    Amount INTEGER,
                    AdvanceToSpaceId INTEGER,
                    FOREIGN KEY (AdvanceToSpaceId) REFERENCES BoardSpace(Id),
                    CardTypeId INTEGER,
                    FOREIGN KEY (CardTypeId) REFERENCES CARDTYPE(Id)
                );

                CREATE TABLE IF NOT EXISTS THEMECARD(
                    ID Serial PRIMARY KEY,
                    CardId INTEGER,
                    FOREIGN KEY (CardId) REFERENCES Card(Id),
                    ThemeId INTEGER,
                    FOREIGN KEY (ThemeId) REFERENCES Theme(Id),
                    CardDescription TEXT
                );

                CREATE TABLE IF NOT EXISTS GAMECARD(
                    ID Serial PRIMARY KEY,
                    CardId INTEGER,
                    FOREIGN KEY (CardId) REFERENCES Card(Id),
                    GameId TEXT,
                    FOREIGN KEY (GameId) REFERENCES Game(Id)
                );
            ";
            db.Execute(sql,transaction);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}