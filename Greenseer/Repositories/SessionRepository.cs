using Greenseer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Repositories;

public sealed class SessionRepository : IRepository<Session>
{
  private readonly IMongoCollection<Session> _sessionCollection;

  public SessionRepository(IOptions<GoalDatabaseOptions> mongoDbSettings)
  {
    var mongoClient = new MongoClient(mongoDbSettings.Value.ConnectionString);
    var database = mongoClient.GetDatabase(mongoDbSettings.Value.DatabaseName);
    _sessionCollection = database.GetCollection<Session>(mongoDbSettings.Value.SessionCollectionName);
  }

  public async Task Create(Session session) => await _sessionCollection.InsertOneAsync(session);

  public async Task Update(string name, Session session) => await _sessionCollection.ReplaceOneAsync(x => x.Name == name, session);

  public async Task<Session?> Get(string name) => await _sessionCollection.Find(x => x.Name == name).FirstOrDefaultAsync();

  public async Task<List<Session>> GetAll() => await _sessionCollection.Find(new BsonDocument()).ToListAsync();
}