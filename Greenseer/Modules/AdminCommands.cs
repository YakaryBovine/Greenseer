using Discord.Interactions;
using Greenseer.Models;
using Greenseer.Services;

namespace Greenseer.Modules;

public sealed class AdminCommands : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IMongoDBService _mongoDbService;

  public AdminCommands(IMongoDBService mongoDbService)
  {
    _mongoDbService = mongoDbService;
  }
  
  [SlashCommand("addgoal", "Adds a new Goal type to the game.")]
  public async Task AddGoal(string name, string description, int pointValue)
  {
    await _mongoDbService.CreateAsync(new Goal
    {
      Name = name,
      Description = description,
      PointValue = pointValue
    });
    await RespondAsync($"Successfully added \"{name}\" to the list of possible Goals.");
  }
}