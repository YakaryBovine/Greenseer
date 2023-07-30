using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

/// <summary>
/// A particular session of a game of Crusader Kings III. Each session has its own set of <see cref="Player"/>.
/// </summary>
public sealed class Session
{
  /// <summary>The name of the active Session.</summary>
  [BsonId]
  public string Name { get; set; } = null!;
  
  /// <summary>All <see cref="Player"/>s registered to this session.</summary>
  public List<Player> Players { get; set; } = new();
}