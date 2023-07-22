using Greenseer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Services;

public sealed class MongoDBService
{
  private readonly IMongoCollection<Goal> _goalCollection;

  public MongoDBService(IOptions<GoalDatabaseOptions> mongoDbSettings)
  {
    var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
    var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
    _goalCollection = database.GetCollection<Goal>(mongoDbSettings.Value.BooksCollectionName);
  }

  public async Task<List<Goal>> GetAsync()
  {
    return await _goalCollection.Find(new BsonDocument()).ToListAsync();
  }

  public async Task CreateAsync(Goal goal)
  {
    await _goalCollection.InsertOneAsync(goal);
  }
}