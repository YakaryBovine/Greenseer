using Discord;
using Discord.Interactions;
using Discord.WebSocket;
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
      HasTarget = hasTarget,
      GoalType = goalType
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
  
  [SlashCommand("givegoal", "Gives the specified Goal to a user.")]
  [RequireUserPermission(GuildPermission.Administrator)]
  public async Task GiveGoal(SocketGuildUser user, string goalName)
  {
    var player = await _mongoDbService.GetPlayer(user.Id.ToString());
    if (player == null)
    {
      await RespondAsync($"{user.Username} is not registered.");
      return;
    }

    var goal = await _mongoDbService.GetGoal(goalName);
    
    if (goal == null)
    {
      await RespondAsync($"There is no goal named {goalName}.");
      return;
    }
    
    player.Goals?.Add(goal);
    await _mongoDbService.UpdatePlayer(player.Id!, player);
    await RespondAsync($"Successfully added Goal {goalName} to {user.Username}.");
  }
}