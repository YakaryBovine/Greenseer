using Discord.Interactions;
using Greenseer.Models;
using Greenseer.Services;

namespace Greenseer.Modules;

public sealed class UserCommands : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IMongoDbService _mongoDbService;

  public UserCommands(IMongoDbService mongoDbService)
  {
    _mongoDbService = mongoDbService;
  }
  
  [SlashCommand("listgoals", "Lists all of the Goals in the game.")]
  public async Task ListGoals()
  {
    var listOfGoals = await _mongoDbService.GetGoals();
    var readableListOfGoals = string.Join(Environment.NewLine, listOfGoals.Select(x => $"**{x.Name} ({x.PointValue})**: {x.Description}"));
    await RespondAsync($"__**Goals**__ {Environment.NewLine}{readableListOfGoals}");
  }

  [SlashCommand("register", "Registers you as a player in the ongoing game.")]
  public async Task Register()
  {
    var user = Context.User;

    if (await _mongoDbService.GetPlayer(user.Username) != null)
    {
      await RespondAsync("You are already registered.");
      return;
    }

    await _mongoDbService.CreatePlayer(new Player
    {
      Id = user.Id.ToString(),
      Name = user.Username,
      Score = 0
    });
    await RespondAsync($"Registered {user.Username} to the ongoing game.");
  }
}