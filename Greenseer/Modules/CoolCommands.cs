using Discord.Interactions;

namespace Greenseer.Modules;

public sealed class CoolCommands : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand("beans", "do beans things")]
  public async Task Beans()
  {
    await RespondAsync("You executed some bullshit");
  }
}