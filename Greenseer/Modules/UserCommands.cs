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
  
  [SlashCommand("allgoals", "Lists all of the Goals in the game.")]
  public async Task AllGoals()
  {
    var listOfGoals = await _mongoDbService.GetGoals();
    var readableListOfGoals = string.Join(Environment.NewLine, listOfGoals.Select(x => $"**{x.Name} ({x.PointValue})**: {x.Description}"));
    await RespondAsync($"__**Goals**__ {Environment.NewLine}{readableListOfGoals}");
  }
  
  [SlashCommand("scores", "Shows the current Scores of every player.")]
  public async Task Scores()
  {
    var listOfPlayers = await _mongoDbService.GetPlayers();
    var orderedPlayers = listOfPlayers.OrderByDescending(x => x.Points);
    var playerScores = string.Join(Environment.NewLine, orderedPlayers.Select(x => $"**{x.Name}**: {x.Points}"));
    await RespondAsync($"__**Scores**__ {Environment.NewLine}{playerScores}");
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
    var eligibleGoals = await _mongoDbService.GetGoals();
    
    foreach (var existingGoal in player.Goals)
    {
      if (eligibleGoals.Contains(existingGoal))
        eligibleGoals.Remove(existingGoal);
    }

    for (var i = 0; i < missingGoals; i++)
    {
      if (eligibleGoals.Count == 0)
      {
        await RespondAsync("There are not enough unique Personal Goals left in the pool to draw a full hand.");
        return;
      }
      var drawnGoal = eligibleGoals[new Random().Next(0, eligibleGoals.Count - 1)];
      eligibleGoals.Remove(drawnGoal);
      player.Goals.Add(drawnGoal);
    }
    
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
      await RespondAsync("You are not registered. Register by using the /register command.", ephemeral: true);
      return;
    }

    var listOfGoals = player.Goals ?? new List<Goal>();

    if (listOfGoals.Count == 0)
    {
      await RespondAsync("You have no Goals. Draw some with the /draw command.", ephemeral: true);
      return;
    }
    
    var readableListOfGoals = string.Join(Environment.NewLine, listOfGoals.Select(x => $"**{x.Name} ({x.PointValue})**: {x.Description}"));
    await RespondAsync($"__**Your Goals**__ {Environment.NewLine}{readableListOfGoals}", ephemeral: true);
  }
  
  [SlashCommand("complete", "Completes the Goal with the specified name.")]
  public async Task Complete(string goalName)
  {
    var user = Context.User;
    var player = await _mongoDbService.GetPlayer(user.Username);
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.");
      return;
    }
    
    player.Goals ??= new List<Goal>();
    var goalToComplete = player.Goals.FirstOrDefault(x => x.Name == goalName);
    if (goalToComplete == null)
    {
      await RespondAsync($"You don't have a Goal named {goalName}.", ephemeral: true);
      return;
    }

    player.Goals.Remove(goalToComplete);
    player.Points += goalToComplete.PointValue;
    await _mongoDbService.UpdatePlayer(player.Id!, player);
    await RespondAsync($"{player.Name} has successfully completed {goalName}! They are awarded {goalToComplete.PointValue} Points.");
  }
  
  [SlashCommand("discard", "Discards the Goal with the specified name.")]
  public async Task Discard(string goalName)
  {
    var user = Context.User;
    var player = await _mongoDbService.GetPlayer(user.Username);
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.");
      return;
    }
    
    player.Goals ??= new List<Goal>();
    var goalToComplete = player.Goals.FirstOrDefault(x => x.Name == goalName);
    if (goalToComplete == null)
    {
      await RespondAsync($"You don't have a Goal named {goalName}.", ephemeral: true);
      return;
    }

    player.Goals.Remove(goalToComplete);
    await _mongoDbService.UpdatePlayer(player.Id!, player);
    await RespondAsync($"{player.Name} has discarded {goalName}.");
  }
}