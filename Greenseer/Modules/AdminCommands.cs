using Discord.Interactions;
using Greenseer.Models;
using Greenseer.Services;

namespace Greenseer.Modules;

public sealed class AdminCommands : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IMongoDbService _mongoDbService;

  public AdminCommands(IMongoDbService mongoDbService)
  {
    _mongoDbService = mongoDbService;
  }

  [SlashCommand("addgoal", "Adds a new Goal type to the game.")]
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

  [SlashCommand("migrategoals", "Fixes all goals in the database with any new data.")]
  public async Task MigrateGoals()
  {
    var allGoals = await _mongoDbService.GetGoals();
    foreach (var goal in allGoals)
    {
      if (goal.GoalType != null) 
        continue;
      goal.GoalType = GoalType.Personal;
      await _mongoDbService.UpdateGoal(goal.Name, goal);
    }

    await RespondAsync($"Successfully migrated Goals.");
  }
}