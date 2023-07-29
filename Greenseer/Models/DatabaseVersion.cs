using MongoDB.Bson.Serialization.Attributes;

namespace Greenseer.Models;

public sealed class DatabaseVersion
{
  public DatabaseVersion(int major, int minor, int build, int revision)
  {
    Version = new Version(major, minor, build, revision);
  }

  public DatabaseVersion()
  {
    
  }
  
  [BsonId]
  public int Id { get; set; }
  
  public Version Version { get; set; } = null!;
}