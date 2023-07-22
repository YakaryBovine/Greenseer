using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

public sealed class Player
{
  [BsonId]
  public string? Id { get; set; }

  public string Name { get; set; } = null!;
  
  public int Score { get; set; }
}