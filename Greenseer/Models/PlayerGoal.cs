using Mongo.Migration.Documents;
using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

/// <summary>A <see cref="Goal"/> held by a player.</summary>
[CollectionLocation("PlayerGoals")]
public sealed class PlayerGoal : IDocument
{
  /// <summary>The name of the <see cref="Goal"/> the player can complete.</summary>
  public string GoalName { get; set; }
  
  /// <summary>The <see cref="Goal"/> the player can complete.</summary>
  [BsonIgnore]
  public Goal Goal { get; set; }
  
  /// <summary>The <see cref="Player"/> the <see cref="Goal"/> is targeted at, if any.</summary>
  public Player? Target { get; set; }
  
  /// <summary>Gets the <see cref="Goal"/> name after formatting rules have been applied.</summary>
  public string GetParsedName() => Goal.Name.Replace("{target}", Target?.Name);
  
  /// <summary>Gets the <see cref="Goal"/> description after formatting rules have been applied.</summary>
  public string GetParsedDescription() => Goal.Description.Replace("{target}", Target?.Name);
  
  /// <summary>Indicates the current version of the entity for migration purposes.</summary>
  public DocumentVersion Version { get; set; }
}