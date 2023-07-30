using Greenseer.Models;
using Greenseer.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Greenseer.Migrations;

public sealed class MovePlayersToDefaultSession : IMigration
{
  public DatabaseVersion Version { get; } = new(2, 0, 1, 0);

  public void Migrate(IMongoDatabase database)
  {
    var playerCollection = database.GetCollection<Player>("Players");
    var sessionCollection = database.GetCollection<Session>("Session");
    var players = playerCollection.Find(new BsonDocument()).ToList();

    var newSession = new Session
    {
      Name = "Nathaniel's Rebellion",
      Players = players
    };
    sessionCollection.InsertOne(newSession);
    
    database.DropCollection("Players");
  }
}