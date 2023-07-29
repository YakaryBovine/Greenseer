using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Greenseer;
using Greenseer.Models;
using Greenseer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mongo.Migration.Startup.DotNetCore;

var config = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .AddEnvironmentVariables()
  .Build();
var client = new DiscordSocketClient();
client.Log += Log;

var interactionService = new InteractionService(client.Rest);
client.Ready += async () => { await interactionService.RegisterCommandsToGuildAsync(1130781212978462772); };

Bootstrapper.ServiceCollection.Configure<GoalDatabaseOptions>(config.GetSection(GoalDatabaseOptions.GoalDatabase));
Bootstrapper.ServiceCollection.AddSingleton(config);
Bootstrapper.ServiceCollection.AddSingleton(client);
Bootstrapper.ServiceCollection.AddSingleton(interactionService);
Bootstrapper.ServiceCollection.AddSingleton<IInteractionHandler, InteractionHandler>();
Bootstrapper.ServiceCollection.AddSingleton<IMongoDbService, MongoDbService>();
Bootstrapper.ServiceCollection.AddMigration();
Bootstrapper.InitializeServiceProvider();

await MainAsync();

async Task MainAsync()
{
  await Bootstrapper.ServiceProvider.GetRequiredService<IInteractionHandler>().InitializeAsync();
  var token = config.GetRequiredSection("Discord")["DiscordBotToken"];
  if (string.IsNullOrWhiteSpace(token))
  {
    await Logger.Log(LogSeverity.Error, $"{nameof(Program)} | {nameof(MainAsync)}", "Token is null or empty.");
    return;
  }
        
  await client.LoginAsync(TokenType.Bot, token);
  await client.StartAsync();
  
  await Task.Delay(Timeout.Infinite);
}

Task Log(LogMessage msg)
{
  Logger.Log(msg);
  return Task.CompletedTask;
}