using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

public sealed class Goal
{
  [BsonId]
  [BsonRepresentation(BsonType.ObjectId)]
  public string? Id { get; set; }

  public string Name { get; set; } = null!;
  
  /// <summary>
  /// What the user needs to do to complete the goal.
  /// </summary>
  public string Description { get; set; } = null!; 
}