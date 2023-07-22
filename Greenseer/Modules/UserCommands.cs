using Discord.Interactions;
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
    var listOfGoals = await _mongoDbService.GetAsync();
    var readableListOfGoals = string.Join(Environment.NewLine, listOfGoals.Select(x => $"**{x.Name} ({x.PointValue})**: {x.Description}"));
    await RespondAsync($"__**Goals**__ {Environment.NewLine}{readableListOfGoals}");
  }
}