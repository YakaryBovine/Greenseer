using Mongo.Migration.Migrations.Database;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Migrations;

public class ChangeGoalsToPlayerGoals : DatabaseMigration
{
  public ChangeGoalsToPlayerGoals() : base("0.0.1")
  {
  }

  public override void Up(IMongoDatabase database)
  {
    var collection = database.GetCollection<BsonDocument>("Player");
    var documents = collection.Find(new BsonDocument()).ToList();
    foreach (var document in documents)
    {
      foreach (var goal in document["Goals"].AsBsonArray)
      {
        goal["Description"] = null;
        goal["PointValue"] = null;
        goal["GoalType"] = null;
      }
    }
  }

  public override void Down(IMongoDatabase database)
  {
    throw new NotImplementedException();
  }
}