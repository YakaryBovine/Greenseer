using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Greenseer;
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
Bootstrapper.RegisterInstance(client);
Bootstrapper.RegisterInstance(interactionService);
Bootstrapper.RegisterType<IInteractionHandler, InteractionHandler>();
Bootstrapper.RegisterInstance(config);

await MainAsync();

async Task MainAsync()
{
  await Bootstrapper.ServiceProvider.GetRequiredService<IInteractionHandler>().InitializeAsync();
  var token = config.GetRequiredSection("Settings")["DiscordBotToken"];
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