using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Greenseer.Exceptions;
using Greenseer.Models;
using Greenseer.Repositories;
using Greenseer.Services;

namespace Greenseer.Modules;

[RequireUserPermission(GuildPermission.Administrator)]
public sealed class AdminCommands : InteractionModuleBase<SocketInteractionContext>
{
  private readonly IMongoDbService _mongoDbService;
  private readonly IRepository<Session> _sessionRepository;
  private readonly IRepository<GlobalSettings> _globalSettingsRepository;
  private const string GlobalSettingsId = "0";

  public AdminCommands(IMongoDbService mongoDbService, IRepository<Session> sessionRepository, IRepository<GlobalSettings> globalSettingsRepository)
  {
    _mongoDbService = mongoDbService;
    _sessionRepository = sessionRepository;
    _globalSettingsRepository = globalSettingsRepository;
  }

  [SlashCommand("registerplayer", "Registers another player in the ongoing game.")]
  public async Task Register(SocketGuildUser user)
  {
    try
    {
      var session = await GetActiveSession();

      if (session.Players.FirstOrDefault(x => x.Id == user.Id.ToString()) != null)
      {
        await RespondAsync($"{user.Username} is already registered to {session.Name}.");
        return;
      }

      session.Players.Add(new Player
      {
        Id = user.Id.ToString(),
        Name = user.Username,
        Points = 0
      });
      await _sessionRepository.Update(session.Name, session);

      await RespondAsync($"Registered {user.Username} to {session.Name}.");
    }
    catch (Exception ex)
    {
      await Logger.Log(LogSeverity.Error, nameof(Register), ex.Message, ex);
    }
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

  [SlashCommand("session", "Changes the active game session to the one with the specified name.")]
  [RequireUserPermission(GuildPermission.Administrator)]
  public async Task SetActiveSession(string sessionName)
  {
    var settings = await _globalSettingsRepository.Get(GlobalSettingsId);

    if (settings == null)
      throw new DocumentNotFoundException(typeof(GlobalSettings), GlobalSettingsId);

    if (settings.ActiveSessionId == sessionName)
    {
      await RespondAsync($"The active game session is already {sessionName}.");
      return;
    }

    var session = await _sessionRepository.Get(sessionName);
    
    if (session == null)
    {
      session = new Session
      {
        Name = sessionName
      };
      await _sessionRepository.Create(session);
      settings.ActiveSessionId = session.Name;
      await _globalSettingsRepository.Update(settings.Id, settings);
      await RespondAsync($"Created a new session named {session.Name}.");
    }
    else
    {
      settings.ActiveSessionId = sessionName;
      await _globalSettingsRepository.Update(settings.Id, settings);

      await RespondAsync($"Active game session changed to {session.Name}.");
    }
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
    var activeSession = await GetActiveSession();
    var player = activeSession.Players.FirstOrDefault(x => x.Id == user.Id.ToString());
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
    
    player.Goals.Add(goal);
    await _sessionRepository.Update(activeSession.Name, activeSession);
    await RespondAsync($"Successfully added Goal {goalName} to {user.Username}.");
  }
  
  private async Task<Session> GetActiveSession()
  {
    var settings = await _globalSettingsRepository.Get(GlobalSettingsId);
    return await _sessionRepository.Get(settings.ActiveSessionId);
  }
}