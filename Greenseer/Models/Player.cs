using Mongo.Migration.Documents;
using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

public sealed class Player : IDocument
{
  /// <summary>The player's ID according to Discord.</summary>
  [BsonId]
  public string? Id { get; set; }

  /// <summary>The player's name according to Discord.</summary>
  public string Name { get; set; } = null!;
  
  /// <summary>The points the player has accumulated over the course of the game.</summary>
  public int Points { get; set; }
  
  /// <summary>The <see cref="Goal"/>s this player can complete to gain points.</summary>
  public List<Goal>? Goals { get; set; }
  
  /// <summary>Indicates the current version of the entity for migration purposes.</summary>
  public DocumentVersion Version { get; set; }
}