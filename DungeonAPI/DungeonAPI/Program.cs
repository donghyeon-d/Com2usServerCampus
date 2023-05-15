using DungeonAPI.Services;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.Configure<AppConfig>(configuration.GetSection(nameof(AppConfig)));
builder.Services.Configure<AdminConfig>(configuration.GetSection(nameof(AdminConfig)));

builder.Services.AddSingleton<IMasterDataDb, MasterDataDb>();
builder.Services.AddSingleton<INoticeMemoryDb, NoticeMemeoryDb>();
builder.Services.AddSingleton<IMemoryDb, MemoryDb>();
builder.Services.AddTransient<IAccountDb, AccountDb>();
builder.Services.AddTransient<IPlayerDb, PlayerDb>();
builder.Services.AddTransient<IItemDb, ItemDb>();
builder.Services.AddTransient<IMailContentDb, MailContentDb>();
builder.Services.AddTransient<IMailDb, MailDb>();
builder.Services.AddTransient<IInAppPurchaseDb, InAppPurchaseDb>();
builder.Services.AddTransient<IAttendanceBookDb, AttendanceBookDb>();
builder.Services.AddTransient<ICompletedDungeonDb, CompletedDungeonDb>();

builder.Services.AddControllers();


var app = builder.Build();

var masterDataDb = app.Services.GetRequiredService<IMasterDataDb>();
var noticeMemoryDb = app.Services.GetRequiredService<INoticeMemoryDb>();
var memoryDb = app.Services.GetRequiredService<IMemoryDb>();


app.UseMiddleware<DungeonAPI.Middleware.CheckAuthAndVersion>();
app.UseRouting();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();

