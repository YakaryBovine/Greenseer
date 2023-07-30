using Discord;
using Discord.Interactions;
using Greenseer.Extensions;
using Greenseer.Models;
using Greenseer.Repositories;
using Greenseer.Services;

namespace Greenseer.Modules;

public sealed class UserCommands : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IMongoDbService _mongoDbService;
  private readonly IRepository<Session> _sessionRepository;
  private readonly IRepository<GlobalSettings> _globalSettingsRepository;
  private const string GlobalSettingsId = "0";
  
  public UserCommands(IMongoDbService mongoDbService, IRepository<Session> sessionRepository, IRepository<GlobalSettings> globalSettingsRepository)
  {
    _mongoDbService = mongoDbService;
    _sessionRepository = sessionRepository;
    _globalSettingsRepository = globalSettingsRepository;
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
    const int goalsPerMessage = 15;
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
    var globalSettings = await _globalSettingsRepository.Get(GlobalSettingsId);
    var session = await _sessionRepository.Get(globalSettings.ActiveSessionId);
    
    try
    {
      var listOfPlayers = session.Players;
      var orderedPlayers = listOfPlayers.OrderByDescending(x => x.Points);
      var playerScores = string.Join(Environment.NewLine, orderedPlayers.Select(x => $"**{x.Name}**: {x.Points}"));
      await RespondAsync($"__**Scores**__ {Environment.NewLine}{playerScores}");
    }
    catch (Exception ex)
    {
      Logger.Log(LogSeverity.Error, nameof(Scores), ex.Message, ex);
    }
  }

  [SlashCommand("register", "Registers you as a player in the ongoing game.")]
  public async Task Register()
  {
    var session = await GetActiveSession();
    var user = Context.User;

    if (session.Players.FirstOrDefault(x => x.Id == user.Id.ToString()) != null)
    {
      await RespondAsync("You are already registered.");
      return;
    }

    session.Players.Add(new Player
    {
      Id = user.Id.ToString(),
      Name = user.Username,
      Points = 0
    });
    _sessionRepository.Update(session.Name, session);
      
    await RespondAsync($"Registered {user.Username} to the ongoing game.");
  }
  
  [SlashCommand("draw", "Draws Personal Goals from the deck until you have 5 total Goals.")]
  public async Task Draw()
  {
    var activeSession = await GetActiveSession();
    var user = Context.User;

    var player = activeSession.Players.FirstOrDefault(x => x.Id == user.Id.ToString());
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.", ephemeral: true);
      return;
    }

    if (player.Goals.Count >= 5)
    {
      await RespondAsync($"You already have {player.Goals.Count} Goals.", ephemeral: true);
      return;
    }

    var missingGoals = 5 - player.Goals.Count;
    var eligibleGoals = (await _mongoDbService.GetGoals())
      .Where(x => x.GoalType == GoalType.Personal)
      .Where(x => !player.Goals.Select(playerGoal => playerGoal.GoalName).Contains(x.Name))
      .ToList();

    var eligiblePlayerTargets = activeSession.Players
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

      eligibleGoals.Remove(drawnGoal);
      player.Goals.Add(new PlayerGoal
      {
        GoalName = drawnGoal.Name,
        Target = drawnGoal.HasTarget ? eligiblePlayerTargets.GetRandom() : null
      });
    }
    
    await _sessionRepository.Update(activeSession.Name, activeSession);
    await RespondAsync($"{player.Name} has successfully drawn up to 5 Goals.");
  }
  
  [SlashCommand("mygoals", "Shows you all of your incomplete Goals.")]
  public async Task MyGoals()
  {
    var user = Context.User;
    var activeSession = await GetActiveSession();

    var player = activeSession.Players.FirstOrDefault(x => x.Id == user.Id.ToString());
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.", ephemeral: true);
      return;
    }

    var listOfGoals = player.Goals;

    if (listOfGoals.Count == 0)
    {
      await RespondAsync("You have no Goals. Draw some with the /draw command.", ephemeral: true);
      return;
    }
    
    var readableListOfGoals = string.Join(Environment.NewLine, listOfGoals.Select(x => $"**{x.GetParsedName()} ({x.Goal.PointValue})**: {x.GetParsedDescription()}"));
    await RespondAsync($"__**Your Goals**__ {Environment.NewLine}{readableListOfGoals}", ephemeral: true);
  }
  
  [SlashCommand("complete", "Completes the Goal with the specified name.")]
  public async Task Complete(string goalName)
  {
    var user = Context.User;
    var activeSession = await GetActiveSession();
    var player = activeSession.Players.FirstOrDefault(x => x.Id == user.Id.ToString());
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.");
      return;
    }

    var universalGoals = (await _mongoDbService.GetGoals())
      .Where(x => x.GoalType is GoalType.Universal)
      .ToList();

    var goalToComplete = universalGoals.FirstOrDefault(x => x.Name == goalName) ??
                         player.Goals.FirstOrDefault(x => x.GetParsedName() == goalName)?.Goal;

    if (goalToComplete == null)
    {
      await RespondAsync($"You don't have a Goal named {goalName}, nor is there a Universal Goal with that name.", ephemeral: true);
      return;
    }

    if (goalToComplete.GoalType is not GoalType.Universal)
      player.Goals.Remove(player.Goals.First(x => x.GoalName == goalToComplete.Name));
    
    player.Points += goalToComplete.PointValue;
    
    await _sessionRepository.Update(activeSession.Name, activeSession);
    await RespondAsync($"{player.Name} has successfully completed {goalName}! They are awarded {goalToComplete.PointValue} Points.");
  }
  
  [SlashCommand("discard", "Discards the Goal with the specified name.")]
  public async Task Discard(string goalName)
  {
    var user = Context.User;
    var activeSession = await GetActiveSession();
    var player = activeSession.Players.FirstOrDefault(x => x.Id == user.Id.ToString());
    
    if (player == null)
    {
      await RespondAsync("You are not registered. Register by using the /register command.");
      return;
    }

    var goalToComplete = player.Goals.FirstOrDefault(x => x.GetParsedName() == goalName);
    if (goalToComplete == null)
    {
      await RespondAsync($"You don't have a Goal named {goalName}.", ephemeral: true);
      return;
    }

    player.Goals.Remove(goalToComplete);
    await _sessionRepository.Update(activeSession.Name, activeSession);
    await RespondAsync($"{player.Name} has discarded {goalName}.");
  }

  private async Task<Session> GetActiveSession()
  {
    var settings = await _globalSettingsRepository.Get(GlobalSettingsId);
    return await _sessionRepository.Get(settings.ActiveSessionId);
  }
}