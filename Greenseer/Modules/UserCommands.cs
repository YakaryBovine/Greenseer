using Discord.Interactions;
using Greenseer.Extensions;
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
  
  [SlashCommand("allgoals", "Lists all Goals of a particular type.")]
  public async Task AllGoals(GoalType goalType)
  {
    var listOfGoals = (await _mongoDbService.GetGoals()).Where(x => x.GoalType == goalType).ToList();

    if (listOfGoals.Count == 0)
    {
      await RespondAsync($"There are no {goalType.ToString()} Goals registered in the system.");
      return;
    }

    var alreadyResponded = false;
    var page = 0;
    const int goalsPerMessage = 10;
    while (listOfGoals.Any())
    {
      if (listOfGoals.Count <= page*goalsPerMessage)
        return;
      
      var goalsToDisplay = listOfGoals.Skip(page*goalsPerMessage).Take(goalsPerMessage);
      var readableListOfGoals = string.Join(Environment.NewLine, goalsToDisplay.Select(x => $"**{x.Name} ({x.PointValue})**: {x.Description}"));
      var responseMessage = $"__**{goalType.ToString()} Goals (Page {page+1})**__ {Environment.NewLine}{readableListOfGoals}";
      if (!alreadyResponded)
      {
        await RespondAsync(responseMessage);
        alreadyResponded = true;
      }
      else
        await FollowupAsync(responseMessage);
      page++;
    }
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

    if (await _mongoDbService.GetPlayer(user.Id.ToString()) != null)
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

    var player = await _mongoDbService.GetPlayer(user.Id.ToString());
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.", ephemeral: true);
      return;
    }

    player.Goals ??= new List<Goal>();

    if (player.Goals.Count >= 5)
    {
      await RespondAsync($"You already have {player.Goals.Count} Goals.", ephemeral: true);
      return;
    }

    var missingGoals = 5 - player.Goals.Count;
    var eligibleGoals = (await _mongoDbService.GetGoals())
      .Where(x => x.GoalType == GoalType.Personal)
      .Where(x => !player.Goals.Select(playerGoal => playerGoal.Name).Contains(x.Name))
      .ToList();

    var eligiblePlayerTargets = (await _mongoDbService.GetPlayers())
      .Where(x => x.Id != player.Id)
      .ToList();
    for (var i = 0; i < missingGoals; i++)
    {
      if (eligibleGoals.Count == 0)
      {
        await RespondAsync("There are not enough unique Personal Goals left in the pool to draw a full hand.");
        return;
      }
      var drawnGoal = eligibleGoals.GetRandom();
      if (drawnGoal.HasTarget) 
        drawnGoal.Target = eligiblePlayerTargets.GetRandom();
      
      eligibleGoals.Remove(drawnGoal);
      player.Goals.Add(drawnGoal);
    }
    
    await _mongoDbService.UpdatePlayer(player.Id!, player);
    
    await RespondAsync($"{player.Name} has successfully drawn up to 5 Goals.");
  }
  
  [SlashCommand("mygoals", "Shows you all of your incomplete Goals.")]
  public async Task MyGoals()
  {
    var user = Context.User;

    var player = await _mongoDbService.GetPlayer(user.Id.ToString());
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
    
    var readableListOfGoals = string.Join(Environment.NewLine, listOfGoals.Select(x => $"**{x.GetParsedName()} ({x.PointValue})**: {x.GetParsedDescription()}"));
    await RespondAsync($"__**Your Goals**__ {Environment.NewLine}{readableListOfGoals}", ephemeral: true);
  }
  
  [SlashCommand("complete", "Completes the Goal with the specified name.")]
  public async Task Complete(string goalName)
  {
    var user = Context.User;
    var player = await _mongoDbService.GetPlayer(user.Id.ToString());
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.");
      return;
    }
    
    player.Goals ??= new List<Goal>();
    var eligibleGoals = (await _mongoDbService.GetGoals())
      .Where(x => x.GoalType is GoalType.Universal)
      .ToList();
    eligibleGoals.AddRange(player.Goals);
    
    var goalToComplete = eligibleGoals.FirstOrDefault(x => x.GetParsedName() == goalName);
    if (goalToComplete == null)
    {
      await RespondAsync($"You don't have a Goal named {goalName}, nor is there a Universal Goal with that name.", ephemeral: true);
      return;
    }

    if (goalToComplete.GoalType is not GoalType.Universal)
      player.Goals.Remove(goalToComplete);
    
    player.Points += goalToComplete.PointValue;
    await _mongoDbService.UpdatePlayer(player.Id!, player);
    await RespondAsync($"{player.Name} has successfully completed {goalName}! They are awarded {goalToComplete.PointValue} Points.");
  }
  
  [SlashCommand("discard", "Discards the Goal with the specified name.")]
  public async Task Discard(string goalName)
  {
    var user = Context.User;
    var player = await _mongoDbService.GetPlayer(user.Id.ToString());
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.");
      return;
    }
    
    player.Goals ??= new List<Goal>();
    var goalToComplete = player.Goals.FirstOrDefault(x => x.GetParsedName() == goalName);
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