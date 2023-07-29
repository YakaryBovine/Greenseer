using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Greenseer;
using Greenseer.Models;
using Greenseer.Repositories;
using Greenseer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

var configurationRoot = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .AddEnvironmentVariables()
  .Build();
var discordSocketClient = new DiscordSocketClient();
discordSocketClient.Log += Log;

var interactionService = new InteractionService(discordSocketClient.Rest);
discordSocketClient.Ready += async () => { await interactionService.RegisterCommandsToGuildAsync(1130781212978462772); };

Bootstrapper.ServiceCollection.Configure<GoalDatabaseOptions>(configurationRoot.GetSection(GoalDatabaseOptions.GoalDatabase));
Bootstrapper.ServiceCollection.AddSingleton(configurationRoot);
Bootstrapper.ServiceCollection.AddSingleton(discordSocketClient);
Bootstrapper.ServiceCollection.AddSingleton(interactionService);
Bootstrapper.ServiceCollection.AddSingleton<IMongoClient>(new MongoClient("mongodb://localhost:27017"));
Bootstrapper.ServiceCollection.AddSingleton<IInteractionHandler, InteractionHandler>();
Bootstrapper.ServiceCollection.AddSingleton<IMongoDbService, MongoDbService>();
Bootstrapper.ServiceCollection.AddSingleton<IRepository<Player>, PlayerRepository>();
Bootstrapper.ServiceCollection.AddSingleton<IMigrationService, MigrationService>();
Bootstrapper.InitializeServiceProvider();
Bootstrapper.ServiceProvider.GetRequiredService<IMigrationService>().Migrate();

await MainAsync();

async Task MainAsync()
{
  await Bootstrapper.ServiceProvider.GetRequiredService<IInteractionHandler>().InitializeAsync();
  var token = configurationRoot.GetRequiredSection("Discord")["DiscordBotToken"];
  if (string.IsNullOrWhiteSpace(token))
  {
    await Logger.Log(LogSeverity.Error, $"{nameof(Program)} | {nameof(MainAsync)}", "Token is null or empty.");
    return;
  }
        
  await discordSocketClient.LoginAsync(TokenType.Bot, token);
  await discordSocketClient.StartAsync();
  
  await Task.Delay(Timeout.Infinite);
}

Task Log(LogMessage msg)
{
  Logger.Log(msg);
  return Task.CompletedTask;
}