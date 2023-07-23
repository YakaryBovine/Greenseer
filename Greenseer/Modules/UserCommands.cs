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
      Points = 0
    });
    await RespondAsync($"Registered {user.Username} to the ongoing game.");
  }
  
  [SlashCommand("draw", "Draws Personal Goals from the deck until you have 5 total Goals.")]
  public async Task Draw()
  {
    var user = Context.User;

    var player = await _mongoDbService.GetPlayer(user.Username);
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.");
      return;
    }

    player.Goals ??= new List<Goal>();

    if (player.Goals.Count >= 5)
    {
      await RespondAsync($"You already have {player.Goals.Count} Goals.");
      return;
    }

    var missingGoals = 5 - player.Goals.Count;
    var availableGoals = await _mongoDbService.GetGoals();
    for (var i = 0; i < missingGoals; i++)
      player.Goals.Add(availableGoals[new Random().Next(0, availableGoals.Count-1)]);

    await _mongoDbService.UpdatePlayer(player.Id!, player);
    
    await RespondAsync("Successfully drew up to 5 Goals.");
  }
  
  [SlashCommand("mygoals", "Shows you all of your incomplete Goals.")]
  public async Task MyGoals()
  {
    var user = Context.User;

    var player = await _mongoDbService.GetPlayer(user.Username);
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.");
      return;
    }

    var listOfGoals = player.Goals ?? new List<Goal>();

    if (listOfGoals.Count == 0)
    {
      await RespondAsync("You have no Goals. Draw some with the /draw command.");
      return;
    }
    
    var readableListOfGoals = string.Join(Environment.NewLine, listOfGoals.Select(x => $"**{x.Name} ({x.PointValue})**: {x.Description}"));
    await RespondAsync($"__**Your Goals**__ {Environment.NewLine}{readableListOfGoals}");
  }
}