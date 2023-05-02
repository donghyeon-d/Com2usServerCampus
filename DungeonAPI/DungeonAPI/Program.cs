using DungeonAPI.Services;
using DungeonAPI.Configs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.Configure<AppConfig>(configuration.GetSection(nameof(AppConfig)));

builder.Services.AddSingleton<IMasterDataDb, MasterDataDb>();
builder.Services.AddTransient<IAccountDb, AccountDb>();
builder.Services.AddTransient<IUser, User>();
builder.Services.AddTransient<IInventory, Inventory>();
builder.Services.AddSingleton<ImemoryDb, RedisDb>();

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

//app.UseAuthorization();
//app.MapControllers();
//app.UseMiddleware<DungeonAPI.Middleware.CheckVersion>();
app.UseRouting();


app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.Run();

