using Greenseer.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Migrations;

public sealed class ChangeGoalsToPlayerGoals : IMigration
{
  public DatabaseVersion Version { get; } = new(0, 0, 0, 4);

  public void Migrate(IMongoDatabase database)
  {
    var collection = database.GetCollection<BsonDocument>("Players");
    var players = collection.Find(new BsonDocument()).ToList();
    foreach (var player in players)
    {
      foreach (var goal in player["Goals"].AsBsonArray)
      {
        var goalDocument = goal.AsBsonDocument;
        goalDocument["GoalName"] = goalDocument["_id"];
        goalDocument.Remove("_id");
      }
      collection.ReplaceOne(x => x["_id"] == player["_id"], player);
    }
  }
}