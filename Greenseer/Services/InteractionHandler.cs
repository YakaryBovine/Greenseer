using System.Reflection;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Greenseer.Services;

public sealed class InteractionHandler : IInteractionHandler
{
  private readonly DiscordSocketClient _client;
  private readonly InteractionService _interactiveService;

  public InteractionHandler(
    DiscordSocketClient client,
    InteractionService interactiveService)
  {
    _client = client;
    _interactiveService = interactiveService;
  }

  public async Task InitializeAsync()
  {
    await _interactiveService.AddModulesAsync(Assembly.GetExecutingAssembly(), Bootstrapper.ServiceProvider);
    
    _client.InteractionCreated += HandleCommandAsync;

    _interactiveService.SlashCommandExecuted += async (_, context, result) =>
    {
      if (!result.IsSuccess && result.ErrorReason != null)
        await context.Channel.SendMessageAsync($"error: {result.ErrorReason}");
    };

    foreach (var module in _interactiveService.Modules)
    {
      await Logger.Log(LogSeverity.Info, $"{nameof(InteractionHandler)} | Commands",
        $"Module '{module.Name}' initialized.");
    }
  }

  private async Task HandleCommandAsync(SocketInteraction socketInteraction)
  {
    var context = new SocketInteractionContext(_client, socketInteraction);
    await _interactiveService.ExecuteCommandAsync(context, Bootstrapper.ServiceProvider);
  }
}