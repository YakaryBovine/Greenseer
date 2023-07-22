using Greenseer.Models;

namespace Greenseer.Services;

public interface IMongoDbService
{
  Task<List<Goal>> GetGoals();
  Task CreateGoal(Goal goal);
  Task DeleteGoal(string name);
  Task<Goal?> GetGoal(string name);
  Task CreatePlayer(Player player);
  Task<Player?> GetPlayer(string userUsername);
}