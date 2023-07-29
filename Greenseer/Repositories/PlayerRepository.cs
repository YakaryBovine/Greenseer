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

  public Task Create(Player player)
  {
    throw new NotImplementedException();
  }

  public Task Update(string id, Player player)
  {
    throw new NotImplementedException();
  }

  public async Task<Player?> Get(string id)
  {
    var player = await _mongoDbService.GetPlayer(id);
    if (player == null)
      return null;
    
    foreach (var goal in player.Goals) 
      goal.Goal = await _mongoDbService.GetGoal(goal.GoalName) ?? throw new Exception();

    return player;
  }

  public Task<List<Player>> GetAll()
  {
    throw new NotImplementedException();
  }
}