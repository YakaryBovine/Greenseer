using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

public sealed class Goal
{
  [BsonId]
  public string Name { get; set; } = null!;
  
  /// <summary>What the user needs to do to complete the goal.</summary>
  public string Description { get; set; } = null!;

  /// <summary>How many points the user gets when they complete the goal.</summary>
  public int PointValue { get; set; }
  
  /// <summary>Whether or not the Goal is targeted at a particular player.</summary>
  public bool HasTarget { get; set; }
  
  /// <summary>The <see cref="Player"/> the <see cref="Goal"/> is targeted at, if any.</summary>
  public Player? Target { get; set; }

  /// <summary>Gets the <see cref="Name"/> after formatting rules have been applied.</summary>
  public string GetParsedName() => Name.Replace("{target}", Target?.Name);
  
  /// <summary>Gets the <see cref="Description"/> after formatting rules have been applied.</summary>
  public string GetParsedDescription() => Description.Replace("{target}", Target?.Name);
}