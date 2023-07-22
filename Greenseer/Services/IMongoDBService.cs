using Greenseer.Models;

namespace Greenseer.Services;

public interface IMongoDBService
{
  public Task<List<Goal>> GetAsync();
  public Task CreateAsync(Goal goal);
}