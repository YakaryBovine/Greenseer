using Discord;
using Discord.Interactions;
using Greenseer.Models;
using Greenseer.Services;

namespace Greenseer.Modules;

[RequireUserPermission(GuildPermission.Administrator)]
public sealed class AdminCommands : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IMongoDbService _mongoDbService;

  public AdminCommands(IMongoDbService mongoDbService)
  {
    _mongoDbService = mongoDbService;
  }

  [SlashCommand("addgoal", "Adds a new Goal type to the game.")]
  [RequireUserPermission(GuildPermission.Administrator)]
  public async Task AddGoal(string name, string description, int pointValue, GoalType goalType = GoalType.Personal, bool hasTarget = false)
  {
    await _mongoDbService.CreateGoal(new Goal
    {
      Name = name,
      Description = description,
      PointValue = pointValue,
      HasTarget = hasTarget
    });
    await RespondAsync($"Successfully added {name} to the list of possible Goals.");
  }

  [SlashCommand("deletegoal", "Deletes the goal with the specified name.")]
  [RequireUserPermission(GuildPermission.Administrator)]
  public async Task DeleteGoal(string name)
  {
    if (await _mongoDbService.GetGoal(name) == null)
    {
      await RespondAsync($"There is no Goal named {name}.");
      return;
    }

    await _mongoDbService.DeleteGoal(name);
    await RespondAsync($"Successfully deleted \"{name}\" from the list of possible Goals.");
  }
}