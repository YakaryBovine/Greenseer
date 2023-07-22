using Discord;
using Discord.Net;
using Discord.WebSocket;
using Greenseer;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

var config = new ConfigurationBuilder()
  .AddJsonFile("appsettings.json")
  .AddEnvironmentVariables()
  .Build();
var client = new DiscordSocketClient();
client.Log += Log;
client.Ready += ClientReady;
client.SlashCommandExecuted += SlashCommandHandler;

// Setup your DI container.
Bootstrapper.Init();
Bootstrapper.RegisterInstance(client);
Bootstrapper.RegisterInstance(config);

await MainAsync();

async Task MainAsync()
{
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

async Task ClientReady()
{
  var globalCommand = new SlashCommandBuilder();
  globalCommand.WithName("beans");
  globalCommand.WithDescription("This is my first global slash command");

  try
  {
    await client.CreateGlobalApplicationCommandAsync(globalCommand.Build());
  }
  catch(HttpException exception)
  {
    var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
    Console.WriteLine(json);
  }
}

Task Log(LogMessage msg)
{
  Logger.Log(msg);
  return Task.CompletedTask;
}

async Task SlashCommandHandler(SocketSlashCommand command)
{
  await command.RespondAsync($"You executed {command.Data.Name}");
}