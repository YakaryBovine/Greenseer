using Greenseer.Models;
using Greenseer.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Migrations;

public sealed class MovePlayersToDefaultSession : IMigration
{
  public DatabaseVersion Version { get; } = new(2, 0, 0, 0);

  public void Migrate(IMongoDatabase database)
  {
    var playerCollection = database.GetCollection<BsonDocument>("Players");
    var sessionCollection = database.GetCollection<BsonDocument>("Session");
    var players = playerCollection.Find(new BsonDocument()).ToList();

    var newSession = new BsonDocument
    {
      ["Players"] = players.ToBson()
    };
    sessionCollection.InsertOne(newSession);
    
    database.DropCollection("Players");
  }
}