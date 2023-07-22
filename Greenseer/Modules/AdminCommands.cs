using Discord.Interactions;
using Greenseer.Models;
using Greenseer.Services;

namespace Greenseer.Modules;

public sealed class CoolCommands : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IMongoDBService _mongoDbService;

  public CoolCommands(IMongoDBService mongoDbService)
  {
    _mongoDbService = mongoDbService;
  }
  
  [SlashCommand("beans", "Adds a new Goal type to the game.")]
  public async Task Beans()
  {
    await _mongoDbService.CreateAsync(new Goal
    {
      Name = "meat",
      Description = "sack"
    });
    await RespondAsync("You executed some bullshit");
  }
}