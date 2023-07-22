using Greenseer.Models;

namespace Greenseer.Services;

public interface IMongoDbService
{
  Task<List<Goal>> GetAsync();
  Task CreateAsync(Goal goal);
  Task DeleteGoal(string name);
  Task<Goal?> GetGoal(string name);
}