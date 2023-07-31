using Greenseer.Exceptions;
using Greenseer.Models;
using Greenseer.Repositories;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Greenseer.Services;

public sealed class MigrationService : IMigrationService
{
  private readonly IRepository<GlobalSettings> _globalSettingsRepository;
  private readonly IMongoDatabase _database;
  private const string GlobalSettingsId = "0";

  public MigrationService(IOptions<GoalDatabaseOptions> mongoDbSettings, IRepository<GlobalSettings> globalSettingsRepository)
  {
    _globalSettingsRepository = globalSettingsRepository;
    var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
    _database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
  }
  
  public async Task Migrate()
  {
    var globalSettings = await _globalSettingsRepository.Get(GlobalSettingsId);

    if (globalSettings == null)
      throw new DocumentNotFoundException(typeof(GlobalSettings), GlobalSettingsId);
    
    var databaseVersion = globalSettings.DatabaseVersion;
    var migrationTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
      .Where(x => typeof(IMigration).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
      .ToList();
    
    var migrationInstances = migrationTypes
      .Select(x => (IMigration)Activator.CreateInstance(x)!)
      .Where(x => x.Version > databaseVersion)
      .ToList();

    foreach (var migrationInstance in migrationInstances) 
      migrationInstance.Migrate(_database);

    if (migrationInstances.Count <= 0) 
      return;
    
    globalSettings.DatabaseVersion = migrationInstances.Select(x => x.Version).Max()!;
    await _globalSettingsRepository.Update(globalSettings.Id, globalSettings);
  }
}