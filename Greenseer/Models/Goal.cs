using Mongo.Migration.Documents;
using Mongo.Migration.Documents.Attributes;
using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

[CollectionLocation("Goals")]
public sealed class Goal : IDocument
{
  [BsonId]
  public string Name { get; set; } = null!;
  
  /// <summary>What the user needs to do to complete the goal.</summary>
  public string Description { get; set; } = null!;

  /// <summary>How many points the user gets when they complete the goal.</summary>
  public int PointValue { get; set; }
  
  /// <summary>Whether or not the Goal is targeted at a particular player.</summary>
  public bool HasTarget { get; set; }

  /// <summary>The type of the <see cref="Goal"/>, which determines some aspects of its behaviour.</summary>
  public GoalType GoalType { get; set; }

  /// <summary>Indicates the current version of the entity for migration purposes.</summary>
  public DocumentVersion Version { get; set; }
}