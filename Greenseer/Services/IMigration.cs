using Greenseer.Models;
using MongoDB.Driver;

namespace Greenseer.Services;

public interface IMigration
{
  /// <summary>
  /// Databases below this version will be migrated, and will have their version increased to this.
  /// </summary>
  public DatabaseVersion Version { get; }

  public void Migrate(IMongoDatabase database);
}