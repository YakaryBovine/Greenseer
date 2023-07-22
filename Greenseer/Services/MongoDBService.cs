using Discord;
using Greenseer.Models;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Services;

public sealed class MongoDbService : IMongoDbService
{
  private readonly IMongoCollection<Goal> _goalCollection;
  private readonly IMongoCollection<Player> _playerCollection;

  public MongoDbService(IOptions<GoalDatabaseOptions> mongoDbSettings)
  {
    var client = new MongoClient(mongoDbSettings.Value.ConnectionString);
    var database = client.GetDatabase(mongoDbSettings.Value.DatabaseName);
    _goalCollection = database.GetCollection<Goal>(mongoDbSettings.Value.GoalsCollectionName);
    _playerCollection = database.GetCollection<Player>(mongoDbSettings.Value.PlayerCollectionName);

    try
    {
      client.GetDatabase("admin").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
      Logger.Log(LogSeverity.Info, nameof(MongoDbService), "Successfully connected to MongoDB deployment.");
    }
    catch (Exception ex)
    {
      Console.WriteLine(ex);
    }
  }

  public async Task CreateGoal(Goal goal) => await _goalCollection.InsertOneAsync(goal);
  
  public async Task<Goal?> GetGoal(string name) => await _goalCollection.Find(x => x.Name == name).FirstOrDefaultAsync();
  
  public async Task<List<Goal>> GetGoals() => await _goalCollection.Find(new BsonDocument()).ToListAsync();
  
  public async Task DeleteGoal(string name) =>
    await _goalCollection.DeleteOneAsync(x => x.Name == name);
  
  public async Task CreatePlayer(Player player) => await _playerCollection.InsertOneAsync(player);
  
  public async Task<Player?> GetPlayer(string name) => await _playerCollection.Find(x => x.Name == name).FirstOrDefaultAsync();
}