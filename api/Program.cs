using System.Data;
using System.Reflection;
using api.Helper;
using api.hub;
using api.Hub.Service;
using api.Interface;
using api.Repository;
using api.Service;
using api.Socket;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Npgsql;
using TypeGen.Core.Generator;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.AddFilter<SocketContextHubFilter<MonopolyHub>>();
});

builder.Services.AddSingleton(typeof(GameState<>), typeof(GameState<>));

// Add Cors Policy
builder.Services.AddCors(o => o.AddPolicy("CorsPolicy", builder =>{
    builder
        .AllowAnyOrigin()  
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

//if no db connection string
if(string.IsNullOrEmpty(builder.Configuration.GetConnectionString("DefaultConnection"))){
    Console.WriteLine("No connection string configured");
    builder.Services.AddScoped<IDbConnection>(sp =>
        new SqliteConnection("Data Source=:memory:") // In-memory SQLite DB
    );
//if found db connection string
}else{
    Console.WriteLine("Found connectionString, setting up database connection.");
    builder.Services.AddScoped<IDbConnection>(sp =>
         new NpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
}

//add repositories for db calls
builder.Services.AddScoped<IBoardSpaceRepository, BoardSpaceRepository>();
builder.Services.AddScoped<IGameCardRepository, GameCardRepository>();
builder.Services.AddScoped<IGameLogRepository,GameLogRepository>();
builder.Services.AddScoped<IGamePropertyRepository,GamePropertyRepository>();
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<ILastDiceRollRepository, LastDiceRollRepository>();
builder.Services.AddScoped<IPlayerIconRepository, PlayerIconRepository>();
builder.Services.AddScoped<IPlayerRepository, PlayerRepository>();
builder.Services.AddScoped<IPlayerTradeRepository, PlayerTradeRepository>();
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<IThemeRepository, ThemeRepository>();
builder.Services.AddScoped<ITradePropertyRepository, TradePropertyRepository>();
builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<ITurnOrderRepository, TurnOrderRepository>();
//socket context
builder.Services.AddScoped<ISocketContextAccessor, SocketContextAccessor>();
//services
builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddScoped<ISocketMessageService, SocketMessageService>();
builder.Services.AddScoped<IGameService, GameService>();

bool isRegistered = builder.Services.Any(sd =>
    sd.ServiceType == typeof(ISocketMessageService));


var app = builder.Build();


//initialize all the tables for the app if needed
using (var scope = app.Services.CreateScope())
{
    var dbConnection = scope.ServiceProvider.GetRequiredService<IDbConnection>();
    dbConnection.Open(); // Open the connection
    DatabaseInitializer.Initialize(dbConnection); // Initialize the database
}

if (app.Environment.IsDevelopment())
{
    // Delete old types
    var outputDir = Path.GetFullPath(@"../ui/src/generated");
    if (Directory.Exists(outputDir))
    {
        Directory.Delete(outputDir, recursive: true);
    }
    //Generate new types
    var options = new GeneratorOptions // create the options object
    {
        BaseOutputDirectory = @"../ui/src/generated",
        TypeNameConverters = [],
        FileNameConverters = [new NoOpFileNameConverter()],
        CsNullableTranslation = TypeGen.Core.StrictNullTypeUnionFlags.Optional,
    }; 
    var generator = new Generator(options); // create the generator instance
    var assembly = Assembly.GetExecutingAssembly(); // get the assembly to generate files for
    generator.Generate(assembly);
    //create barrel file
    var files = Directory.GetFiles(outputDir, "*.ts", SearchOption.TopDirectoryOnly)
        .Where(f => !f.EndsWith("index.ts"))
        .Select(Path.GetFileNameWithoutExtension);
    var lines = files.Select(f => $"export * from './{f}';");
    File.WriteAllLines(Path.Combine(outputDir, "index.ts"), lines);

    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
//app.UseAuthorization();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseCors("CorsPolicy");
app.MapHub<MonopolyHub>("/monopoly");
app.MapControllers();
app.Run();
