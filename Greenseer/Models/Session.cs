using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

/// <summary>
/// A particular session of a game of Crusader Kings III. Each session has its own set of <see cref="Player"/>.
/// </summary>
public sealed class Session
{
  [BsonId]
  public string? Id { get; set; }
  
  /// <summary>All <see cref="Player"/>s registered to this session.</summary>
  public List<Player> Goals { get; set; } = null!;
}