using System.Text.Json;
using ZLogger;
using DungeonAPI.Services;
using DungeonAPI.Configs;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
SetConfigure(builder);

AddService(builder);

SettingLogger();

var app = builder.Build();

AddZLogger();

DBInit();

app.UseMiddleware<DungeonAPI.Middleware.CheckAuthAndVersion>();
app.UseRouting();
app.UseEndpoints(endpoints => { endpoints.MapControllers(); });

app.Run();

void SetConfigure(WebApplicationBuilder? builder)
{
    builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
    builder.Services.Configure<AppConfig>(configuration.GetSection(nameof(AppConfig)));
    builder.Services.Configure<AdminConfig>(configuration.GetSection(nameof(AdminConfig)));
}

void AddService(WebApplicationBuilder? builder)
{
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
}

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

    logging.AddZLoggerRollingFile(
        (dt, x) => $"{fileDir}{dt.ToLocalTime():yyyy-MM-dd}_{x:000}.log",
        x => x.ToLocalTime().Date, 1024,
        options =>
        {
            options.EnableStructuredLogging = true;
            var time = JsonEncodedText.Encode("Timestamp");
            //DateTime.Now는 UTC+0 이고 한국은 UTC+9이므로 9시간을 더한 값을 출력한다.
            var timeValue = JsonEncodedText.Encode(DateTime.Now.AddHours(9).ToString("yyyy/MM/dd HH:mm:ss"));

            options.StructuredLoggingFormatter = (writer, info) =>
            {
                writer.WriteString(time, timeValue);
                info.WriteToJsonWriter(writer);
            };
        }); // 1024KB

    logging.AddZLoggerConsole(options =>
    {
        options.EnableStructuredLogging = true;
        var time = JsonEncodedText.Encode("EventTime");
        var timeValue = JsonEncodedText.Encode(DateTime.Now.AddHours(9).ToString("yyyy/MM/dd HH:mm:ss"));

        options.StructuredLoggingFormatter = (writer, info) =>
        {
            writer.WriteString(time, timeValue);
            info.WriteToJsonWriter(writer);
        };
    });
}

void AddZLogger()
{
    var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
    loggerFactory.CreateLogger("Global");

}

void DBInit()
{
    var masterDataDb = app.Services.GetRequiredService<IMasterDataDb>();
    var noticeMemoryDb = app.Services.GetRequiredService<INoticeMemoryDb>();
    var memoryDb = app.Services.GetRequiredService<IMemoryDb>();
}