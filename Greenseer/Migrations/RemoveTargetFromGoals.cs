using Greenseer.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Migrations;

public sealed class RemoveTargetFromGoals : IMigration
{
  public DatabaseVersion Version { get; } = new(0, 0, 0, 5);

  public void Migrate(IMongoDatabase database)
  {
    var collection = database.GetCollection<BsonDocument>("Goals");
    var goals = collection.Find(new BsonDocument()).ToList();
    foreach (var goal in goals)
    {
      goal.Remove("Target");
      collection.ReplaceOne(x => x["_id"] == goal["_id"], goal);
    }
  }
}