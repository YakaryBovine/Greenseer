using Greenseer.Models;

namespace Greenseer.Services;

public interface IMongoDbService
{
  Task CreateGoal(Goal goal);
  Task<List<Goal>> GetGoals();
  Task UpdateGoal(string name, Goal goal);
  Task DeleteGoal(string name);
  Task<Goal?> GetGoal(string name);
}