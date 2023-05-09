using DungeonAPI.Services;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;
using ZLogger;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.Configure<AppConfig>(configuration.GetSection(nameof(AppConfig)));
builder.Services.Configure<AdminConfig>(configuration.GetSection(nameof(AdminConfig)));

builder.Services.AddSingleton<IMasterDataDb, MasterDataDb>();
builder.Services.AddTransient<IAccountDb, AccountDb>();
builder.Services.AddTransient<IPlayerDb, PlayerDb>();
builder.Services.AddTransient<IItemDb, ItemDb>();
builder.Services.AddTransient<IInAppPurchaseDb, InAppPurchaseDb>();
builder.Services.AddTransient<IMailDb, MailDb>();
builder.Services.AddTransient<IMailContentDb, MailContentDb>();
builder.Services.AddTransient<IMailRewardDb, MailRewardDb>();
builder.Services.AddTransient<IAttendanceBookDb, AttendanceBookDb>();
builder.Services.AddTransient<IItemEnhanceDb, ItemEnhanceDb>();
builder.Services.AddSingleton<INoticeDb, NoticeDb>();
builder.Services.AddSingleton<IAuthUserDb, AuthUserDb>();

builder.Services.AddControllers();

SettingLogger();

var app = builder.Build();

//app.UseAuthorization();
app.UseMiddleware<DungeonAPI.Middleware.CheckVersion>();
app.UseMiddleware<DungeonAPI.Middleware.CheckAuth>();
app.UseRouting();
app.MapControllers();


//app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
app.Run();


void SettingLogger()
{
    var logging = builder.Logging;
    logging.ClearProviders();

    var fileDir = configuration["logdir"];

    var exists = Directory.Exists(fileDir);

    if (!exists)
    {
        Directory.CreateDirectory(fileDir);
    }

    logging.SetMinimumLevel(LogLevel.Information);

    logging.AddZLoggerConsole();

    logging.AddZLoggerFile("fileName.log");

    logging.AddZLoggerRollingFile(
        (dt, x) => $"{fileDir}{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log",
        x => x.ToLocalTime().Date,
        1024);

    logging.AddZLoggerConsole(options =>
    {
        options.EnableStructuredLogging = true;
    });
}

