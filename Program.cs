using EpicGamesBot;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient();
builder.Services.AddTransient<IEpicGamesService, EpicGamesService>();
builder.Services.AddTransient<DiscordService>();

var host = builder.Build();

var discordService = host.Services.GetRequiredService<DiscordService>();
await discordService.SendDiscordMessage();