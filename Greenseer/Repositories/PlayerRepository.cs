using Greenseer.Models;
using Greenseer.Services;

namespace Greenseer.Repositories;

public sealed class PlayerRepository : IRepository<Player>
{
  private readonly IMongoDbService _mongoDbService;

  public PlayerRepository(IMongoDbService mongoDbService)
  {
    _mongoDbService = mongoDbService;
  }

  public async Task Create(Player player) => await _mongoDbService.CreatePlayer(player);

  public async Task Update(string id, Player player) => await _mongoDbService.UpdatePlayer(id, player);

  public async Task<Player?> Get(string id)
  {
    var player = await _mongoDbService.GetPlayer(id);
    if (player == null)
      return null;
    
    foreach (var goal in player.Goals) 
      goal.Goal = await _mongoDbService.GetGoal(goal.GoalName) ?? throw new Exception();

    return player;
  }

  public async Task<List<Player>> GetAll()
  {
    var players = await _mongoDbService.GetPlayers();
    foreach (var player in players)
    {
      foreach (var goal in player.Goals) 
        goal.Goal = await _mongoDbService.GetGoal(goal.GoalName) ?? throw new Exception();
    }
    
    return players;
  }
}