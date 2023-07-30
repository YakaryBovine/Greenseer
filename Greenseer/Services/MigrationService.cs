using Greenseer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Greenseer.Services;

public sealed class MigrationService : IMigrationService
{
  private readonly IMongoCollection<DatabaseVersion> _versionNumberCollection;
  private readonly IMongoDatabase _database;
  private const int VersionNumberId = 0;

  public MigrationService(IOptions<GoalDatabaseOptions> mongoDbSettings)
  {
    var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
    _database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
    _versionNumberCollection = _database.GetCollection<DatabaseVersion>(mongoDbSettings.Value.DatabaseVersionCollectionName);
  }
  
  public void Migrate()
  {
    var databaseVersion = GetDatabaseVersion();
    var migrationTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
      .Where(x => typeof(IMigration).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
      .ToList();
    
    var migrationInstances = migrationTypes
      .Select(x => (IMigration)Activator.CreateInstance(x)!)
      .Where(x => x.Version.Version > databaseVersion.Version)
      .ToList();
    
    foreach (var migrationInstance in migrationInstances) 
      migrationInstance.Migrate(_database);

    if (migrationInstances.Count <= 0) 
      return;
    
    var newVersionNumber = migrationInstances.Select(x => x.Version).Max();
    _versionNumberCollection!.ReplaceOne(x => x!.Id == VersionNumberId, newVersionNumber, new ReplaceOptions
    {
      IsUpsert = true
    });
  }

  private DatabaseVersion GetDatabaseVersion() => _versionNumberCollection.Find(x => x.Id == VersionNumberId).FirstOrDefault() ??
                                                  new DatabaseVersion(0, 0, 0, 0);
}