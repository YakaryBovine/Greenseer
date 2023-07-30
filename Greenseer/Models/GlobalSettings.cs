using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

/// <summary>Bot settings that affect the core operation of the bot.</summary>
public sealed class GlobalSettings
{
  [BsonId]
  public string Id { get; set; }
  
  public string ActiveSessionId { get; set; }
}