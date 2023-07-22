using Discord.Interactions;
using Greenseer.Models;
using Greenseer.Services;

namespace Greenseer.Modules;

public sealed class UserCommands : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IMongoDBService _mongoDbService;

  public UserCommands(IMongoDBService mongoDbService)
  {
    _mongoDbService = mongoDbService;
  }
  
  [SlashCommand("listgoals", "Lists all of the Goals in the game.")]
  public async Task ListGoals()
  {
    var listOfGoals = await _mongoDbService.GetAsync();
    var readableListOfGoals = string.Join(Environment.NewLine, listOfGoals.Select(x => x.Name));
    await RespondAsync($"**Goals** {Environment.NewLine}{readableListOfGoals}");
  }
}