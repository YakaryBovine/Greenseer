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
  
  [SlashCommand("addgoal", "Adds a new Goal type to the game.")]
  public async Task AddGoal(string name)
  {
    await _mongoDbService.CreateAsync(new Goal
    {
      Name = name,
      Description = "sack"
    });
    await RespondAsync($"Successfully added {name} to the list of possible Goals.");
  }
}