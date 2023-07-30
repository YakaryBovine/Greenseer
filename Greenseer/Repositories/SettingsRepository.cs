using Greenseer.Models;

namespace Greenseer.Repositories;

public sealed class GlobalSettingsRepository : IRepository<GlobalSettings>
{
  public Task Create(GlobalSettings player)
  {
    throw new NotImplementedException();
  }

  public Task Update(string id, GlobalSettings player)
  {
    throw new NotImplementedException();
  }

  public Task<GlobalSettings?> Get(string id)
  {
    throw new NotImplementedException();
  }

  public Task<List<GlobalSettings>> GetAll()
  {
    throw new NotImplementedException();
  }
}