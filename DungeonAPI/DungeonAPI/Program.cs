﻿using DungeonAPI.Services;
using DungeonAPI.Configs;
using DungeonAPI.ModelDB;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = builder.Configuration;
builder.Services.Configure<DbConfig>(configuration.GetSection(nameof(DbConfig)));
builder.Services.Configure<AppConfig>(configuration.GetSection(nameof(AppConfig)));
builder.Services.Configure<AdminConfig>(configuration.GetSection(nameof(AdminConfig)));

builder.Services.AddSingleton<IMasterDataDb, MasterDataDb>();
builder.Services.AddTransient<IAccountDb, AccountDb>();
builder.Services.AddTransient<IPlayerDb, PlayerDb>();
builder.Services.AddTransient<IItemDb, ItemDb>();
builder.Services.AddTransient<IMailContentDb, MailContentDb>();
builder.Services.AddTransient<IMailRewardDb, MailRewardDb>();
builder.Services.AddTransient<IMailDb, MailDb>();
builder.Services.AddTransient<IInAppPurchaseDb, InAppPurchaseDb>();
builder.Services.AddTransient<IAttendanceBookDb, AttendanceBookDb>();
builder.Services.AddSingleton<INoticeMemoryDb, NoticeMemeoryDb>();
builder.Services.AddSingleton<IMemoryDb, MemoryDb>();

builder.Services.AddControllers();

var app = builder.Build();

//app.UseMiddleware<DungeonAPI.Middleware.CheckAuthAndVersion>();
app.UseRouting();
app.MapControllers();

app.Run();

