using Greenseer.Models;
using Greenseer.Repositories;

namespace Greenseer.Services;

public sealed class DatabaseSeedingService : IDatabaseSeedingService
{
  private readonly IRepository<GlobalSettings> _globalSettingsRepository;

  public DatabaseSeedingService(IRepository<GlobalSettings> globalSettingsRepository)
  {
    _globalSettingsRepository = globalSettingsRepository;
  }
  
  public async Task SeedDatabaseAsync()
  {
    var settings = (await _globalSettingsRepository.GetAll()).FirstOrDefault() ?? new GlobalSettings
    {
      Id = "0",
      DatabaseVersion = new Version()
    };
    await _globalSettingsRepository.Update(settings.Id, settings);
  }
}