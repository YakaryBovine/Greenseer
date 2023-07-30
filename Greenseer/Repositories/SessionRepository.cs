using Greenseer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Greenseer.Repositories;

public sealed class SessionRepository
{
  private readonly IMongoCollection<Goal> _sessionCollection;

  public SessionRepository(IOptions<GoalDatabaseOptions> mongoDbSettings)
  {
    var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
    var database = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
    _sessionCollection = database.GetCollection<Goal>(mongoDbSettings.Value.SessionCollectionName);
  }
}