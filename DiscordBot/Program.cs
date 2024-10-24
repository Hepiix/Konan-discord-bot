﻿global using Discord;
global using Discord.Interactions;
global using Discord.WebSocket;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
using DiscordBot.Modules.AnimeFeed;
using DiscordBot.Modules.AntiSpam;
using DiscordBot.Modules.BirthdayAnime;
using DiscordBot.Modules.Config;
using DiscordBot.Modules.RaiderIO;
using DiscordBot.Modules.Tarot;
using DiscordBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var builder = new HostApplicationBuilder(args);

builder.Configuration.AddEnvironmentVariables("DNetTemplate_");

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"logs/log-{DateTime.Now:yy.MM.dd_HH.mm}.log")
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(loggerConfig, dispose: true);

builder.Services.AddSingleton(new DiscordSocketClient(
    new DiscordSocketConfig
    {
        GatewayIntents = GatewayIntents.AllUnprivileged,
        FormatUsersInBidirectionalUnicode = false,
        // Add GatewayIntents.GuildMembers to the GatewayIntents and change this to true if you want to download all users on startup
        AlwaysDownloadUsers = false,
        LogGatewayIntentWarnings = false,
        LogLevel = LogSeverity.Info
    }));

builder.Services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), new InteractionServiceConfig()
{
    LogLevel = LogSeverity.Info
}));

builder.Services.AddHostedService<DiscordBotService>();
builder.Services.AddSingleton<TimerService>();
builder.Services.AddSingleton<DiscordChatService>();
builder.Services.AddSingleton<InteractionHandler>();
builder.Services.AddSingleton<TarotService>();
builder.Services.AddSingleton<ConfigBotService>();
builder.Services.AddSingleton<AnimeFeedService>();
builder.Services.AddSingleton<AntiSpamService>();
builder.Services.AddSingleton<RaiderIOService>();

builder.Services.AddSingleton(x => new AnimeListService(x.GetRequiredService<DiscordChatService>(),x.GetRequiredService<ConfigBotService>()));
builder.Services.AddSingleton<BirthdayAnimeService>();

var app = builder.Build();

await app.RunAsync();