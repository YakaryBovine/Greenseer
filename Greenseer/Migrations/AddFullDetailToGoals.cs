using Greenseer.Models;
using Greenseer.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Migrations;

public sealed class AddFullDetailToGoals : IMigration
{
  public Version Version { get; } = new(2, 0, 8, 0);

  public void Migrate(IMongoDatabase database)
  {
    var sessionCollection = database.GetCollection<Session>("Session");

    var allSessions = sessionCollection.Find(new BsonDocument()).ToList();
    
    var allPlayerGoals = allSessions
      .SelectMany(x => x.Players)
      .SelectMany(x => x.Goals);

    foreach (var playerGoal in allPlayerGoals)
    {
      playerGoal.ToBsonDocument().Remove("GoalName");
    }
      
    foreach (var session in allSessions)
      sessionCollection.ReplaceOne(x => x.Name == session.Name, session);
  }
}