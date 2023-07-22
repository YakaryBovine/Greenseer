using Discord.Interactions;
using Greenseer.Models;
using Greenseer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Greenseer.Modules;

public sealed class CoolCommands : InteractionModuleBase<SocketInteractionContext>
{
  [SlashCommand("beans", "Adds a new Goal type to the game.")]
  public async Task Beans()
  {
    var mongoDbService = Bootstrapper.ServiceProvider.GetRequiredService(typeof(IMongoDBService)) as MongoDBService;
    mongoDbService?.CreateAsync(new Goal
    {
      Name = "meat",
      Description = "sack"
    });
    await RespondAsync("You executed some bullshit");
  }
}