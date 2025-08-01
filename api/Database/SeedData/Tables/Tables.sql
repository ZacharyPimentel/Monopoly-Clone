CREATE TABLE IF NOT EXISTS THEME(
    Id INTEGER PRIMARY KEY,
    ThemeName TEXT,
    PrimaryColor TEXT,
    SecondaryColor TEXT
);

CREATE TABLE IF NOT EXISTS GAME(
    Id UUID PRIMARY KEY,
    DiceRollInProgress BOOLEAN DEFAULT false,
    GameName TEXT UNIQUE,
    Password TEXT,
    MovementInProgress BOOLEAN DEFAULT false,
    InLobby BOOLEAN DEFAULT true,
    GameOver BOOLEAN DEFAULT false,
    GameStarted BOOLEAN DEFAULT false,
    StartingMoney INTEGER DEFAULT 1500,
    ThemeId INTEGER NOT NULL,
    FOREIGN KEY (ThemeId) REFERENCES THEME(Id),
    FullSetDoublePropertyRent BOOLEAN DEFAULT false,
    ExtraMoneyForLandingOnGo BOOLEAN DEFAULT false,
    CollectMoneyFromFreeParking BOOLEAN DEFAULT false,
    MoneyInFreeParking INTEGER DEFAULT 0,
    CreatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS GAMEPASSWORD(
    Id Serial PRIMARY KEY,
    GameId UUID NOT NULL,
    FOREIGN KEY (GameId) REFERENCES Game(Id),
    Password TEXT NOT NULL,
    CreatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS LASTDICEROLL(
    Id SERIAL PRIMARY KEY,
    GameId UUID,
    FOREIGN KEY (GameId) REFERENCES GAME(Id),
    DiceOne INTEGER DEFAULT 1,
    DiceTwo INTEGER DEFAULT 1,
    UtilityDiceOne INTEGER,
    UtilityDiceTwo INTEGER,
    CreatedAt TimeStamp NOT NULL,
    UpdatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS PLAYERICON(
    Id INTEGER PRIMARY KEY,
    IconUrl TEXT,
    IconName TEXT
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
    Id UUID PRIMARY KEY,
    PlayerName TEXT,
    Active BOOLEAN DEFAULT true,
    Money INTEGER,
    BoardSpaceId Integer DEFAULT 1,
    PreviousBoardSpaceId Integer DEFAULT 1,
    FOREIGN KEY (BoardSpaceId) REFERENCES BoardSpace(Id),
    IconId Integer NULL CHECK (IconId BETWEEN 1 AND 8),
    InCurrentGame BOOLEAN DEFAULT false,
    IsReadyToPlay BOOLEAN DEFAULT false,
    RollCount INTEGER CHECK (RollCount BETWEEN 0 AND 3) DEFAULT 0,
    CanRoll BOOLEAN DEFAULT false,
    InJail BOOLEAN DEFAULT false,
    GameId UUID,
    FOREIGN KEY (GameId) REFERENCES Game(Id),
    RollingForUtilities BOOLEAN DEFAULT false,
    JailTurnCount INTEGER DEFAULT 0,
    GetOutOfJailFreeCards INTEGER DEFAULT 0,
    Bankrupt BOOLEAN DEFAULT false,
    CreatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS PLAYERDEBT(
    Id SERIAL PRIMARY KEY,
    PlayerId UUID,
    FOREIGN KEY (PlayerId) REFERENCES Player(Id),
    InDebtTo UUID,
    DebtPaid BOOLEAN DEFAULT false,
    Amount INTEGER,
    CreatedAt TimeStamp NOT NULL
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

CREATE TABLE IF NOT EXISTS THEMEPROPERTY(
    Id SERIAL PRIMARY KEY,
    ThemeId INTEGER,
    FOREIGN KEY (ThemeId) REFERENCES Theme(Id),
    PropertyId INTEGER,
    FOREIGN KEY (PropertyId) REFERENCES Property(Id),
    SetNumber INTEGER CHECK (SetNumber BETWEEN 1 AND 8),
    Color TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS TURNORDER(
    Id UUID PRIMARY KEY,
    PlayerId UUID,
    FOREIGN KEY (PlayerId) REFERENCES PLAYER(Id),
    GameId UUID,
    FOREIGN KEY (GameId) REFERENCES GAME(Id),
    PlayOrder INTEGER,
    HasPlayed BOOLEAN DEFAULT false,
    CreatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS GAMEPROPERTY(
    Id SERIAL PRIMARY KEY,
    PlayerId UUID,
    FOREIGN KEY (PlayerId) REFERENCES Player(Id),
    GameId UUID,
    FOREIGN KEY (GameId) REFERENCES Game(Id),
    PropertyId INTEGER,
    FOREIGN KEY (PropertyId) REFERENCES Property(Id),
    UpgradeCount INTEGER CHECK (UpgradeCount BETWEEN 0 AND 5) DEFAULT 0,
    Mortgaged BOOLEAN DEFAULT false,
    CreatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS GAMELOG(
    Id SERIAL PRIMARY KEY,
    GameId UUID NOT NULL,
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
    Played BOOLEAN DEFAULT FALSE,
    PlayedAt TimeStamp,
    FOREIGN KEY (CardId) REFERENCES Card(Id),
    GameId UUID,
    FOREIGN KEY (GameId) REFERENCES Game(Id),
    CreatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS TRADE(
    ID Serial PRIMARY KEY,
    GameId UUID,
    InitiatedBy UUID,
    LastUpdatedBy UUID,
    DeclinedBy UUID,
    AcceptedBy UUID,
    FOREIGN KEY (GameId) REFERENCES Game(Id),
    CreatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS PLAYERTRADE(
    ID Serial PRIMARY KEY,
    TradeId INTEGER,
    FOREIGN KEY (TradeId) REFERENCES Trade(Id),
    PlayerId UUID,
    FOREIGN KEY (PlayerId) REFERENCES Player(Id),
    Money Integer DEFAULT 0,
    GetOutOfJailFreeCards INTEGER DEFAULT 0,
    CreatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS TRADEPROPERTY(
    ID Serial PRIMARY KEY,
    GamePropertyId INTEGER,
    FOREIGN KEY (GamePropertyId) REFERENCES GameProperty(Id),
    PlayerTradeId INTEGER,
    FOREIGN KEY (PlayerTradeId) REFERENCES PlayerTrade(Id),
    CreatedAt TimeStamp NOT NULL
);

CREATE TABLE IF NOT EXISTS COLORGROUP(
    ID Serial PRIMARY KEY,
    GroupName TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS THEMECOLOR(
    ID Serial PRIMARY KEY,
    ThemeId INTEGER,
    FOREIGN KEY (ThemeId) REFERENCES Theme(Id),
    ColorGroupId INTEGER,
    FOREIGN KEY (ColorGroupId) REFERENCES ColorGroup(Id),
    Color TEXT,
    Shade INTEGER CHECK (Shade BETWEEN 1 AND 10)
);

CREATE TABLE IF NOT EXISTS ERRORLOG(
    ID SERIAL PRIMARY KEY,
    ErrorMessage TEXT,
    Source TEXT,
    StackTrace TEXT,
    InnerException JSONB,
    CreatedAt TimeStamp NOT NULL
)