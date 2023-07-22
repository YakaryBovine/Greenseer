using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Greenseer;
using Greenseer.Models;
using Greenseer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var config = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .AddEnvironmentVariables()
  .Build();
var client = new DiscordSocketClient();
client.Log += Log;

var interactionService = new InteractionService(client.Rest);

Bootstrapper.Init();
Bootstrapper.RegisterInstance(config);
Bootstrapper.RegisterInstance(client);
Bootstrapper.RegisterInstance(interactionService);
Bootstrapper._serviceCollection.Configure<GoalDatabaseOptions>(config.GetSection(GoalDatabaseOptions.GoalDatabase));
Bootstrapper.RegisterType<IInteractionHandler, InteractionHandler>();
Bootstrapper.RegisterType<IMongoDBService, MongoDBService>();

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