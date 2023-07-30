using Greenseer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Repositories;

public sealed class GlobalSettingsRepository : IRepository<GlobalSettings>
{
  private readonly IMongoCollection<GlobalSettings> _globalSettingsCollection;

  public GlobalSettingsRepository(IOptions<GoalDatabaseOptions> mongoDbSettings)
  {
    var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
    var database = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
    _globalSettingsCollection = database.GetCollection<GlobalSettings>(mongoDbSettings.Value.GlobalSettingsCollectionName);
  }
  
  public async Task Create(GlobalSettings globalSettings) => await _globalSettingsCollection.InsertOneAsync(globalSettings);

  public async Task Update(string id, GlobalSettings globalSettings) => await _globalSettingsCollection.ReplaceOneAsync(
    x => x.Id == id, globalSettings, new ReplaceOptions
  {
    IsUpsert = true
  });

  public async Task<GlobalSettings?> Get(string id) => await _globalSettingsCollection.Find(x => x.Id == id).FirstOrDefaultAsync();

  public async Task<List<GlobalSettings>> GetAll() => await _globalSettingsCollection.Find(new BsonDocument()).ToListAsync();
}